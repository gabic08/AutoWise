using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace AutoWise.CommonUtilities.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var httpStatusCode = exception switch
        {
            NotImplementedException => HttpStatusCode.NotImplemented,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            NotFoundException => HttpStatusCode.NotFound,
            BadRequestException => HttpStatusCode.BadRequest,
            BadRequestWithMultipleFailuresException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        Dictionary<string, string[]> errorMessages = [];
        if (exception is BadRequestWithMultipleFailuresException)
        {
            errorMessages = JsonSerializer.Deserialize<Dictionary<string, string[]>>(exception.Message);
        }

        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError(exception, "An error occurred while processing the request. Status Code '{statusCode}'. Exception message '{exceptionMeesage}'. Utc time of occurence: {time}",
                (int)httpStatusCode, exception.Message, DateTime.UtcNow);
        }

        httpContext.Response.StatusCode = (int)httpStatusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var errorResponse = new
        {
            Title = exception.GetType().Name,
            ErrorMessage = exception.Message,
            ErrorMessages = errorMessages,
            StatusCode = (int)httpStatusCode,
            TraceId = httpContext.TraceIdentifier,
            Instance = httpContext.Request.Path.Value
        };

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }
}

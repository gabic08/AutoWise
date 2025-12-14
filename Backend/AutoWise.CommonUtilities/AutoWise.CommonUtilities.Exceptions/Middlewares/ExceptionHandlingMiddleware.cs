using System.Net;
using System.Text.Json;

namespace AutoWise.CommonUtilities.Exceptions.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {

            var httpStatusCode = ex switch
            {
                NotImplementedException => HttpStatusCode.NotImplemented,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotFoundException => HttpStatusCode.NotFound,
                BadRequestException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };


            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Request failed with Status Code '{statusCode}' and exception message '{exceptionMeesage}'", (int)httpStatusCode, ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception occurred: {Message}", ex.InnerException.Message);
                }
            }

            context.Response.StatusCode = (int)httpStatusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                ex.Message,
                HttpStatusCode = (int)httpStatusCode
            };

            var response = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(response);
        }
    }
}

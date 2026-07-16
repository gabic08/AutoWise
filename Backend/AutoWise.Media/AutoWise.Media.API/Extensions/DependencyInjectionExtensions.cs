using AutoWise.CommonUtilities.Exceptions.Handler;

namespace AutoWise.Media.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddExceptionHandler<CustomExceptionHandler>();

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseExceptionHandler(options => { });
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}

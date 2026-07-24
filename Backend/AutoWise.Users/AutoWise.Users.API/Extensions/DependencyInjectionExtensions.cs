using AutoWise.CommonUtilities.Exceptions.Handler;
using AutoWise.Users.Infrastructure.Grpc.Services;

namespace AutoWise.Users.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddGrpc();
        services.AddExceptionHandler<CustomExceptionHandler>();

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseExceptionHandler(options => { });
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGrpcService<UserGrpcService>();

        return app;
    }
}

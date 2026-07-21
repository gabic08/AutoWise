namespace AutoWise.UserVehicles.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserVehiclesService, UserVehiclesService>();
        services.AddScoped<IUserVehicleEventService, UserVehicleEventService>();
        services.AddScoped<IUserVehicleAttachmentService, UserVehicleAttachmentService>();

        return services;
    }
}

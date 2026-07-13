using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Services;

namespace AutoWise.UserVehicles.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserVehiclesService, UserVehiclesService>();
        services.AddScoped<IUserVehicleEventService, UserVehicleEventService>();

        return services;
    }
}

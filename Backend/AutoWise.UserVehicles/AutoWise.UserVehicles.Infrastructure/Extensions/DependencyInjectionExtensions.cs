using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using AutoWise.UserVehicles.Application.Data;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using AutoWise.UserVehicles.Infrastructure.Data;
using AutoWise.UserVehicles.Infrastructure.ExternalServices;
using AutoWise.VehiclesCatalog.API.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutoWise.UserVehicles.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();


        services.AddDbContext<UserVehiclesDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IUserVehiclesDbContext, UserVehiclesDbContext>();


        services.AddGrpcClient<VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient>(options =>
        {
            options.Address = new Uri(configuration.GetSection("GrpcSettings:VehiclesCatalogUrl").Value!);
        });
        services.AddScoped<IVehicleSpecificationsService, VehicleSpecificationsGrpcClient>();


        return services;
    }
}

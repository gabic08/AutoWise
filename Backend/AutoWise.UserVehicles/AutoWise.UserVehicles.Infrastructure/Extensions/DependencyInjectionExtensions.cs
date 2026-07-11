using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using AutoWise.UserVehicles.Infrastructure.Data;
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
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found."));
        });

        return services;
    }
}

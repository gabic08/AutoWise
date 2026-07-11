using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using AutoWise.UserVehicles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");

        services.AddDbContext<UserVehiclesDbContext>(options =>
        {
            options.AddInterceptors(new AuditableEntityInterceptor());
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}

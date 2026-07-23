using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using AutoWise.Users.Application;
using AutoWise.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoWise.Users.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUsersDbContext>(sp => sp.GetRequiredService<UsersDbContext>());

        return services;
    }
}

namespace AutoWise.Users.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuditableEntityInterceptor();

        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUsersDbContext>(sp => sp.GetRequiredService<UsersDbContext>());

        services.AddGrpc();

        return services;
    }
}

namespace AutoWise.UserVehicles.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddDbContext<UserVehiclesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.")));

        services.AddScoped<IAuditableDbContext>(sp => sp.GetRequiredService<UserVehiclesDbContext>());

        return services;
    }
}

namespace AutoWise.UserVehicles.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuditableEntityInterceptor();


        services.AddDbContext<UserVehiclesDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IUserVehiclesDbContext>(sp => sp.GetRequiredService<UserVehiclesDbContext>());

        services.AddMassTransitMessaging<UserVehiclesDbContext>(
            configuration,
            OutboxDatabaseProvider.Postgres,
            x => x.AddConsumer<MediaAttachmentUploadedConsumer>());

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        });

        services.AddGrpcClient<VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient>(options =>
        {
            options.Address = new Uri(configuration.GetSection("GrpcSettings:VehiclesCatalogUrl").Value!);
        });
        services.AddScoped<IVehicleSpecificationsService, VehicleSpecificationsGrpcClient>();


        return services;
    }
}

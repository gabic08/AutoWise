namespace AutoWise.Media.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IMediaDbContext>(sp => sp.GetRequiredService<MediaDbContext>());


        services.AddMassTransitMessaging<MediaDbContext>(
            configuration,
            OutboxDatabaseProvider.Postgres,
            x => x.AddConsumer<MediaAttachmentRemovedConsumer>());


        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<LocalDiskStorageOptions>(configuration.GetSection(LocalDiskStorageOptions.SectionName));
        services.Configure<AmazonS3StorageOptions>(configuration.GetSection(AmazonS3StorageOptions.SectionName));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));

        services.AddSingleton<IFileStorageProviderResolver, FileStorageProviderResolver>();
        services.AddSingleton<IFileStorageProvider, LocalDiskStorageProvider>();
        services.AddSingleton<IFileStorageProvider, AmazonS3StorageProvider>();
        services.AddSingleton<IFileStorageProvider, AzureBlobStorageProvider>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Options = sp.GetRequiredService<IOptions<AmazonS3StorageOptions>>().Value;
            var credentials = new BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey);
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(s3Options.Region)
            };

            return new AmazonS3Client(credentials, config);
        });

        services.AddSingleton(sp =>
        {
            var blobOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
            return new BlobServiceClient(blobOptions.ConnectionString);
        });

        return services;
    }
}

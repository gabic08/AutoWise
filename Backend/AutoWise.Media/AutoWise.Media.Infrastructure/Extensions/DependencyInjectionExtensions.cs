using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using AutoWise.Media.Application.Data;
using AutoWise.Media.Application.Storage;
using AutoWise.Media.Infrastructure.Data;
using AutoWise.Media.Infrastructure.Storage;
using AutoWise.Media.Infrastructure.Storage.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        services.AddScoped<IMediaDbContext, MediaDbContext>();


        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<LocalDiskStorageOptions>(configuration.GetSection(LocalDiskStorageOptions.SectionName));
        services.Configure<AmazonS3StorageOptions>(configuration.GetSection(AmazonS3StorageOptions.SectionName));

        services.AddSingleton<IFileStorageProviderResolver, FileStorageProviderResolver>();
        services.AddSingleton<IFileStorageProvider, LocalDiskStorageProvider>();
        services.AddSingleton<IFileStorageProvider, AmazonS3StorageProvider>();

        return services;
    }
}

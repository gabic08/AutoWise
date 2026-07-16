using AutoWise.Media.Application.Config;
using AutoWise.Media.Application.Features.MediaAttachments;

namespace AutoWise.Media.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMediaAttachmentService, MediaAttachmentService>();

        services.Configure<MediaUploadOptions>(options =>
            options.AllowedMimeTypes = configuration.GetSection(MediaUploadOptions.SectionName).Get<List<string>>() ?? []);

        return services;
    }
}

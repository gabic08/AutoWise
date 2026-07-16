using AutoWise.Media.Application.Dtos;
using AutoWise.Media.Domain.Models;
using System.Security.Cryptography;

namespace AutoWise.Media.Application.Features.MediaAttachments;

public partial class MediaAttachmentService
{
    private async Task<Guid> SaveNewMediaFileAsync(UploadMediaRequest request, string contentHash, CancellationToken ct)
    {
        var fileExtension = Path.GetExtension(request.FileName).TrimStart('.');
        var storageKey = $"{contentHash[..2]}/{contentHash[2..4]}/{contentHash}.{fileExtension}";

        var provider = storageProviderResolver.ResolveActiveProvider();
        await provider.SaveAsync(request.Content, storageKey, ct);

        var mediaFile = MediaFile.Create(
            contentHash, request.ContentType, fileExtension, request.Content.Length, provider.ProviderType, storageKey);

        await dbContext.MediaFiles.AddAsync(mediaFile, ct);

        return mediaFile.Id;
    }

    private static async Task<string> ComputeHashAsync(Stream content, CancellationToken ct)
    {
        var hashBytes = await SHA256.HashDataAsync(content, ct);
        content.Position = 0;
        return Convert.ToHexStringLower(hashBytes);
    }
}

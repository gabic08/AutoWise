using AutoWise.CommonUtilities.Exceptions;
using AutoWise.Media.Application.Config;
using AutoWise.Media.Application.Data;
using AutoWise.Media.Application.Dtos;
using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AutoWise.Media.Application.Features.MediaAttachments;

public class MediaAttachmentService(
    IMediaDbContext dbContext,
    IFileStorageProviderResolver storageProviderResolver,
    IOptions<MediaUploadOptions> mediaUploadOptions)
    : IMediaAttachmentService
{
    public async Task<Guid> UploadAsync(UploadMediaRequest request, CancellationToken ct = default)
    {
        if (!mediaUploadOptions.Value.AllowedMimeTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException($"Content type '{request.ContentType}' is not allowed.");
        }

        var contentHash = await ComputeHashAsync(request.Content, ct);

        var existingMediaFile = await dbContext.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(mf => mf.ContentHash == contentHash, ct);

        var mediaFileId = existingMediaFile?.Id ?? await SaveNewMediaFileAsync(request, contentHash, ct);

        var mediaAttachment = MediaAttachment.Create(mediaFileId, request.ParentType, request.ParentEntityId, request.FileName);

        await dbContext.MediaAttachments.AddAsync(mediaAttachment, ct);
        await dbContext.SaveChangesAsync(ct);

        return mediaAttachment.Id;
    }

    public async Task<MediaAttachmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var attachment = await dbContext.MediaAttachments
            .AsNoTracking()
            .Include(ma => ma.MediaFile)
            .FirstOrDefaultAsync(ma => ma.Id == id, ct)
            ?? throw new NotFoundException($"Media attachment with id '{id}' was not found.");

        return new MediaAttachmentResponse(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.MediaFile.ContentType,
            attachment.MediaFile.SizeInBytes,
            attachment.ParentType,
            attachment.ParentEntityId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var attachment = await dbContext.MediaAttachments.FirstOrDefaultAsync(ma => ma.Id == id, ct)
            ?? throw new NotFoundException($"Media attachment with id '{id}' was not found.");

        var mediaFileId = attachment.MediaFileId;

        dbContext.MediaAttachments.Remove(attachment);
        await dbContext.SaveChangesAsync(ct);

        var hasRemainingReferences = await dbContext.MediaAttachments
            .AsNoTracking()
            .AnyAsync(ma => ma.MediaFileId == mediaFileId, ct);

        if (hasRemainingReferences)
        {
            return;
        }

        var mediaFile = await dbContext.MediaFiles.FirstOrDefaultAsync(mf => mf.Id == mediaFileId, ct);
        if (mediaFile is null)
        {
            return;
        }

        var provider = storageProviderResolver.Resolve(mediaFile.StorageProvider);
        await provider.DeleteAsync(mediaFile.StorageKey, ct);

        dbContext.MediaFiles.Remove(mediaFile);
        await dbContext.SaveChangesAsync(ct);
    }

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
using AutoWise.CommonUtilities.Exceptions;
using AutoWise.Media.Application.Config;
using AutoWise.Media.Application.Data;
using AutoWise.Media.Application.Dtos;
using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AutoWise.Media.Application.Features.MediaAttachments;

public partial class MediaAttachmentService(
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
}
using AutoWise.CommonUtilities.Models.BaseEntities;
using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Domain.Models;

public class MediaFile : CreatedAuditBaseEntity
{
    public string ContentHash { get; private set; }
    public string ContentType { get; private set; }
    public string FileExtension { get; private set; }
    public long SizeInBytes { get; private set; }
    public MediaStorageProvider StorageProvider { get; private set; }
    public string StorageKey { get; private set; }

    private readonly List<MediaAttachment> _mediaAttachments = [];
    public IReadOnlyCollection<MediaAttachment> MediaAttachments => _mediaAttachments.AsReadOnly();

    private MediaFile() { }


    public static MediaFile Create(string contentHash, string contentType, string fileExtension, long sizeInBytes, MediaStorageProvider storageProvider, string storageKey)
    {
        var mediaFile = new MediaFile
        {
            Id = Guid.NewGuid()
        };

        mediaFile.SetContentHash(contentHash);
        mediaFile.SetContentType(contentType);
        mediaFile.SetFileExtension(fileExtension);
        mediaFile.SetSizeInBytes(sizeInBytes);
        mediaFile.StorageProvider = storageProvider;
        mediaFile.SetStorageKey(storageKey);

        return mediaFile;
    }

    private void SetContentHash(string contentHash)
    {
        if (string.IsNullOrWhiteSpace(contentHash))
        {
            throw new ArgumentException("Content hash is required.", nameof(contentHash));
        }
        if (contentHash.Length != 64)
        {
            throw new ArgumentOutOfRangeException(nameof(contentHash), "Content hash must be a 64-character SHA-256 hex string.");
        }
        ContentHash = contentHash.Trim().ToLowerInvariant();
    }

    private void SetContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }
        ContentType = contentType.Trim();
    }

    private void SetFileExtension(string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            throw new ArgumentException("File extension is required.", nameof(fileExtension));
        }
        FileExtension = fileExtension.Trim().TrimStart('.').ToLowerInvariant();
    }

    private void SetSizeInBytes(long sizeInBytes)
    {
        if (sizeInBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Size must be greater than zero.");
        }
        SizeInBytes = sizeInBytes;
    }

    private void SetStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }
        StorageKey = storageKey.Trim();
    }
}

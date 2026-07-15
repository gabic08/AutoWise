using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Infrastructure.Storage;

public class AmazonS3StorageProvider : IFileStorageProvider
{
    public MediaStorageProvider ProviderType => MediaStorageProvider.AmazonS3;

    public Task SaveAsync(Stream content, string storageKey, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

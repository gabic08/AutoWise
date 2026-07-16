using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Enums;
using AutoWise.Media.Infrastructure.Storage.Config;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace AutoWise.Media.Infrastructure.Storage;

public class AzureBlobStorageProvider(BlobServiceClient blobServiceClient, IOptions<AzureBlobStorageOptions> options)
    : IFileStorageProvider
{
    public MediaStorageProvider ProviderType => MediaStorageProvider.AzureBlob;


    public async Task SaveAsync(Stream content, string storageKey, CancellationToken ct = default)
    {
        var blobClient = GetBlobClient(storageKey);
        await blobClient.UploadAsync(content, overwrite: true, ct);
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var blobClient = GetBlobClient(storageKey);
        return await blobClient.OpenReadAsync(cancellationToken: ct);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var blobClient = GetBlobClient(storageKey);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    private BlobClient GetBlobClient(string storageKey)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        return containerClient.GetBlobClient(storageKey);
    }
}

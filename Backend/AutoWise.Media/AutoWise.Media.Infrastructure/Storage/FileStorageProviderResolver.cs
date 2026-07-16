using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Enums;
using AutoWise.Media.Infrastructure.Storage.Config;
using Microsoft.Extensions.Options;

namespace AutoWise.Media.Infrastructure.Storage;

public class FileStorageProviderResolver(IEnumerable<IFileStorageProvider> providers, IOptions<StorageOptions> storageOptions)
    : IFileStorageProviderResolver
{
    public IFileStorageProvider Resolve(MediaStorageProvider providerType)
    {
        return providers.FirstOrDefault(p => p.ProviderType == providerType)
            ?? throw new InvalidOperationException($"No storage provider registered for '{providerType}'.");
    }

    public IFileStorageProvider ResolveActiveProvider()
    {
        return Resolve(storageOptions.Value.ActiveProvider);
    }
}

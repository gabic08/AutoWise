using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Infrastructure.Storage;

public class FileStorageProviderResolver(IEnumerable<IFileStorageProvider> providers) : IFileStorageProviderResolver
{
    public IFileStorageProvider Resolve(MediaStorageProvider providerType)
    {
        return providers.FirstOrDefault(p => p.ProviderType == providerType)
            ?? throw new InvalidOperationException($"No storage provider registered for '{providerType}'.");
    }
}

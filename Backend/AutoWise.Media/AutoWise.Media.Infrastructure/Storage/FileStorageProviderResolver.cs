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

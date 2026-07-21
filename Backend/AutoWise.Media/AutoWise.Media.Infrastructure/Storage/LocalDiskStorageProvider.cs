namespace AutoWise.Media.Infrastructure.Storage;

public class LocalDiskStorageProvider(IOptions<LocalDiskStorageOptions> options) : IFileStorageProvider
{
    public MediaStorageProvider ProviderType => MediaStorageProvider.LocalDisk;

    public async Task SaveAsync(Stream content, string storageKey, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(storageKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Media file not found at '{storageKey}'.");
        }

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string storageKey)
    {
        var rootPath = options.Value.RootPath;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidOperationException("Storage:LocalDisk:RootPath is not configured.");
        }

        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(rootPath, normalizedKey);
    }
}
using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Application.Storage;

public interface IFileStorageProvider
{
    MediaStorageProvider ProviderType { get; }

    Task SaveAsync(Stream content, string storageKey, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);
}

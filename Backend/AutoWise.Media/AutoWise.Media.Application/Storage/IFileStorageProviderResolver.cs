namespace AutoWise.Media.Application.Storage;

public interface IFileStorageProviderResolver
{
    IFileStorageProvider Resolve(MediaStorageProvider providerType);
    IFileStorageProvider ResolveActiveProvider();
}

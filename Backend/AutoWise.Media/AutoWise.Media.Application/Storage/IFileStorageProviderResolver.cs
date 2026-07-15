using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Application.Storage;

public interface IFileStorageProviderResolver
{
    IFileStorageProvider Resolve(MediaStorageProvider providerType);
}

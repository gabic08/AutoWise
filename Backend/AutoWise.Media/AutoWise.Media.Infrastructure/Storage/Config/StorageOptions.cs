using AutoWise.Media.Domain.Enums;

namespace AutoWise.Media.Infrastructure.Storage.Config;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public MediaStorageProvider ActiveProvider { get; set; }
}

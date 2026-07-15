namespace AutoWise.Media.Infrastructure.Storage.Config;

public class LocalDiskStorageOptions
{
    public const string SectionName = "Storage:LocalDisk";

    public string RootPath { get; set; }
}

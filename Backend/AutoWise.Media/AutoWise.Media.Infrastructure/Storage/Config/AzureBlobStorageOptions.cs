namespace AutoWise.Media.Infrastructure.Storage.Config;

public class AzureBlobStorageOptions
{
    public const string SectionName = "Storage:AzureBlob";

    public string ConnectionString { get; set; }
    public string ContainerName { get; set; }
}

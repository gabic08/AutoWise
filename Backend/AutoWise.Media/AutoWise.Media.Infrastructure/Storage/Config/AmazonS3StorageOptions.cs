namespace AutoWise.Media.Infrastructure.Storage.Config;

public class AmazonS3StorageOptions
{
    public const string SectionName = "Storage:AmazonS3";

    public string AccessKey { get; set; }
    public string BucketName { get; set; }
    public string Region { get; set; }
    public string SecretKey { get; set; }
}

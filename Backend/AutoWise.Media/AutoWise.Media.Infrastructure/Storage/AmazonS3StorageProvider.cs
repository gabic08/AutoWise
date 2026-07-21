namespace AutoWise.Media.Infrastructure.Storage;

public class AmazonS3StorageProvider(IAmazonS3 s3Client, IOptions<AmazonS3StorageOptions> options)
    : IFileStorageProvider
{
    public MediaStorageProvider ProviderType => MediaStorageProvider.AmazonS3;

    public async Task SaveAsync(Stream content, string storageKey, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = options.Value.BucketName,
            Key = storageKey,
            InputStream = content
        };

        await s3Client.PutObjectAsync(request, ct);
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = options.Value.BucketName,
            Key = storageKey
        };

        var response = await s3Client.GetObjectAsync(request, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = options.Value.BucketName,
            Key = storageKey
        };

        await s3Client.DeleteObjectAsync(request, ct);
    }
}

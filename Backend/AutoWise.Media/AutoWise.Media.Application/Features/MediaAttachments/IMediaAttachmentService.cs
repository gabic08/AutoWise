namespace AutoWise.Media.Application.Features.MediaAttachments;

public interface IMediaAttachmentService
{
    Task<Guid> UploadAsync(UploadMediaRequest request, CancellationToken ct = default);
    Task<MediaAttachmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MediaDownloadResult> DownloadAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
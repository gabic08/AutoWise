namespace AutoWise.Media.Application.Data;

public interface IMediaDbContext : IBaseDbContext
{
    DbSet<MediaFile> MediaFiles { get; }
    DbSet<MediaAttachment> MediaAttachments { get; }
}

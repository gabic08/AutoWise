namespace AutoWise.Media.Infrastructure.Consumers;

public class MediaAttachmentRemovedConsumer(IMediaAttachmentService mediaAttachmentService) : IConsumer<MediaAttachmentRemoved>
{
    public async Task Consume(ConsumeContext<MediaAttachmentRemoved> context)
    {
        await mediaAttachmentService.DeleteAsync(context.Message.MediaAttachmentId, context.CancellationToken);
    }
}

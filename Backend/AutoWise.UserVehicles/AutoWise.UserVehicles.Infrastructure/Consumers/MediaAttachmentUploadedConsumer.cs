namespace AutoWise.UserVehicles.Infrastructure.Consumers;

public class MediaAttachmentUploadedConsumer(IUserVehiclesDbContext dbContext) : IConsumer<MediaAttachmentUploaded>
{
    private const string UserVehicleParentType = "UserVehicle";

    public async Task Consume(ConsumeContext<MediaAttachmentUploaded> context)
    {
        var message = context.Message;

        if (message.ParentType.NotEqualsCaseInsensitive(UserVehicleParentType))
        {
            return;
        }

        var userVehicle = await dbContext.UserVehicles
            .FirstOrDefaultAsync(uv => uv.Id == message.ParentEntityId, context.CancellationToken)
            ?? throw new NotFoundException($"User vehicle with id '{message.ParentEntityId}' was not found for media attachment '{message.MediaAttachmentId}'.");

        userVehicle.AddAttachment(message.MediaAttachmentId, message.OriginalFileName, message.ContentType, message.SizeInBytes);

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

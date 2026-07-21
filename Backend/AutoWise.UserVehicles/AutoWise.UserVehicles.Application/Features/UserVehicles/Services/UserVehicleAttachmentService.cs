namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Services;

public class UserVehicleAttachmentService(IUserVehiclesDbContext dbContext, IEventPublisher eventPublisher)
    : IUserVehicleAttachmentService
{
    public async Task RemoveAttachmentAsync(Guid vehicleId, Guid attachmentId, CancellationToken ct = default)
    {
        var vehicle = await dbContext.UserVehicles
            .Include(uv => uv.UserVehicleAttachments.Where(ua => ua.Id == attachmentId))
            .FirstOrDefaultAsync(uv => uv.Id == vehicleId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{vehicleId}' was not found.");

        if (!vehicle.UserVehicleAttachments.Any(a => a.Id == attachmentId))
        {
            throw new NotFoundException($"Attachment with id '{attachmentId}' was not found.");
        }

        var removedAttachment = vehicle.RemoveAttachment(attachmentId);

        await eventPublisher.PublishAsync(new MediaAttachmentRemoved(removedAttachment.MediaAttachmentId), ct);

        await dbContext.SaveChangesAsync(ct);
    }
}
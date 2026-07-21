namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;

public interface IUserVehicleAttachmentService
{
    Task RemoveAttachmentAsync(Guid vehicleId, Guid attachmentId, CancellationToken ct = default);
}

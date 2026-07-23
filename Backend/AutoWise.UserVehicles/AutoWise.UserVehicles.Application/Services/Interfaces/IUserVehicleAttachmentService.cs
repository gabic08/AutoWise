namespace AutoWise.UserVehicles.Application.Services.Interfaces;

public interface IUserVehicleAttachmentService
{
    Task RemoveAttachmentAsync(Guid vehicleId, Guid attachmentId, CancellationToken ct = default);
}

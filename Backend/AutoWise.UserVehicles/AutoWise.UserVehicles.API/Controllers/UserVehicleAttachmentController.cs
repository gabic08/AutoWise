namespace AutoWise.UserVehicles.API.Controllers;

[Route("api")]
[ApiController]
public class UserVehicleAttachmentController(IUserVehicleAttachmentService userVehicleAttachmentService) : ControllerBase
{
    private readonly IUserVehicleAttachmentService _userVehicleAttachmentService = userVehicleAttachmentService;

    [HttpDelete("user-vehicles/{vehicleId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> RemoveUserVehicleAttachmentAsync(Guid vehicleId, Guid attachmentId, CancellationToken cancellationToken)
    {
        await _userVehicleAttachmentService.RemoveAttachmentAsync(vehicleId, attachmentId, cancellationToken);
        return NoContent();
    }
}

namespace AutoWise.UserVehicles.API.Controllers;

[Route("api")]
[ApiController]
public class UserVehicleController(IUserVehiclesService userVehicleService) : ControllerBase
{
    private readonly IUserVehiclesService _userVehiclesService = userVehicleService;

    [HttpPost("user-vehicles")]
    public async Task<IActionResult> AddUserVehicleAsync(CreateUserVehicleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userVehiclesService.CreateAsync(request, GetSessionUserId(), cancellationToken));
    }

    [HttpGet("user-vehicles")]
    public async Task<IActionResult> GetUserVehiclesAsync([FromQuery] GetUserVehiclesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userVehiclesService.GetAllForUserAsync(GetSessionUserId(), request, cancellationToken));
    }

    [HttpGet("user-vehicles/{id:guid}")]
    public async Task<IActionResult> GetUserVehicleAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userVehiclesService.GetByIdAsync(id, GetSessionUserId(), cancellationToken));
    }

    [HttpPut("user-vehicles/{id:guid}")]
    public async Task<IActionResult> UpdateUserVehicleAsync(Guid id, UpdateUserVehicleRequest request, CancellationToken cancellationToken)
    {
        await _userVehiclesService.UpdateAsync(id, request, GetSessionUserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("user-vehicles/{id:guid}")]
    public async Task<IActionResult> DeleteUserVehicleAsync(Guid id, CancellationToken cancellationToken)
    {
        await _userVehiclesService.DeleteAsync(id, GetSessionUserId(), cancellationToken);
        return NoContent();
    }

    private Guid GetSessionUserId()
    {
        var headerValue = Request.Headers["X-User-Id"].FirstOrDefault();
        if (!Guid.TryParse(headerValue, out var userId))
        {
            throw new UnauthorizedAccessException("Missing or invalid X-User-Id header.");
        }

        return userId;
    }
}

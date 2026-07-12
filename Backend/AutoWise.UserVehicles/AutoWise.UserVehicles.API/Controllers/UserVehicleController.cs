using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AutoWise.UserVehicles.API.Controllers;

[Route("api")]
[ApiController]
public class UserVehicleController(IUserVehicleService userVehicleService) : ControllerBase
{
    private readonly IUserVehicleService _userVehicleService = userVehicleService;
    private readonly Guid temporaryUserId = new("3670849e-1a32-4383-aec2-c6186482c8f7");

    [HttpPost("user-vehicles")]
    public async Task<IActionResult> AddUserVehicleAsync(CreateUserVehicleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userVehicleService.AddUserVehicleAsync(request, temporaryUserId, cancellationToken));
    }
}

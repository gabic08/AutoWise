using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoWise.UserVehicles.API.Controllers;

[Route("api")]
[ApiController]
public class UserVehicleController(IUserVehiclesService userVehicleService) : ControllerBase
{
    private readonly IUserVehiclesService _userVehiclesService = userVehicleService;
    private readonly Guid temporaryUserId = new("54cf3f84-ef0b-47e7-9480-a6e5d0be9052");

    [HttpPost("user-vehicles")]
    public async Task<IActionResult> AddUserVehicleAsync(CreateUserVehicleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userVehiclesService.CreateAsync(request, temporaryUserId, cancellationToken));
    }

    [HttpGet("user-vehicles/{id:guid}")]
    public async Task<IActionResult> GetUserVehicleAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userVehiclesService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPut("user-vehicles/{id:guid}")]
    public async Task<IActionResult> UpdateUserVehicleAsync(Guid id, UpdateUserVehicleRequest request, CancellationToken cancellationToken)
    {
        await _userVehiclesService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("user-vehicles/{id:guid}")]
    public async Task<IActionResult> DeleteUserVehicleAsync(Guid id, CancellationToken cancellationToken)
    {
        await _userVehiclesService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

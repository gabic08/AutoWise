using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoWise.UserVehicles.API.Controllers;

[Route("api")]
[ApiController]
public class UserVehicleEventController(IUserVehicleEventService userVehicleEventService) : ControllerBase
{
    private readonly IUserVehicleEventService _userVehicleEventService = userVehicleEventService;

    [HttpPost("user-vehicles/{vehicleId:guid}/events")]
    public async Task<IActionResult> AddUserVehicleEventAsync(Guid vehicleId, CreateUserVehicleEventRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userVehicleEventService.AddEventAsync(vehicleId, request, cancellationToken));
    }

    [HttpGet("user-vehicles/{vehicleId:guid}/events/{eventId:guid}")]
    public async Task<IActionResult> GetUserVehicleEventAsync(Guid vehicleId, Guid eventId, CancellationToken cancellationToken)
    {
        return Ok(await _userVehicleEventService.GetEventAsync(vehicleId, eventId, cancellationToken));
    }

    [HttpPut("user-vehicles/{vehicleId:guid}/events/{eventId:guid}")]
    public async Task<IActionResult> UpdateUserVehicleEventAsync(Guid vehicleId, Guid eventId, UpdateUserVehicleEventRequest request, CancellationToken cancellationToken)
    {
        await _userVehicleEventService.UpdateEventAsync(vehicleId, eventId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("user-vehicles/{vehicleId:guid}/events/{eventId:guid}")]
    public async Task<IActionResult> RemoveUserVehicleEventAsync(Guid vehicleId, Guid eventId, CancellationToken cancellationToken)
    {
        await _userVehicleEventService.RemoveEventAsync(vehicleId, eventId, cancellationToken);
        return NoContent();
    }
}

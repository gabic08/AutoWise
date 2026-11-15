using AutoWise.Users.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoWise.Users.API.Controllers;

[Route("api")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("users/current-user")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByIdAsync(GetSessionUserId(), cancellationToken));
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

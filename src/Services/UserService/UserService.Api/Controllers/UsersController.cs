using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _userService.GetByIdAsync(userId.Value, ct);
        if (user == null)
            return NotFound(new { success = false, error = new { code = "USER_NOT_FOUND", message = "User not found." } });

        return Ok(new { success = true, data = user });
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _userService.UpdateProfileAsync(userId.Value, request, ct);
        if (user == null)
            return NotFound(new { success = false, error = new { code = "USER_NOT_FOUND", message = "User not found." } });

        return Ok(new { success = true, data = user });
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateAccount(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _userService.DeactivateAccountAsync(userId.Value, ct);
        if (!success)
            return NotFound(new { success = false, error = new { code = "USER_NOT_FOUND", message = "User not found." } });

        return Ok(new { success = true, message = "Account deactivated successfully." });
    }
}

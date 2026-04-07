using AmCart.UserService.Application;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var result = await _authService.RegisterAsync(request, baseUrl, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Created(string.Empty, new { success = true, data = new { user = result.User, tokens = result.Tokens }, message = result.Message });
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        var result = await _authService.VerifyEmailAsync(token, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken ct)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var result = await _authService.ResendVerificationEmailAsync(request.Email, baseUrl, ct);
        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("external/google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalGoogle([FromBody] ExternalLoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await _authService.ExternalLoginAsync(ExternalAuthProvider.Google, request, ipAddress, userAgent, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, data = new { user = result.User, tokens = result.Tokens } });
    }

    [HttpPost("external/facebook")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalFacebook([FromBody] ExternalLoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await _authService.ExternalLoginAsync(ExternalAuthProvider.Facebook, request, ipAddress, userAgent, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, data = new { user = result.User, tokens = result.Tokens } });
    }

    [HttpPost("external/twitter")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalTwitter([FromBody] ExternalLoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await _authService.ExternalLoginAsync(ExternalAuthProvider.Twitter, request, ipAddress, userAgent, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, data = new { user = result.User, tokens = result.Tokens } });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, userAgent, ct);
        if (!result.Success)
            return Unauthorized(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, data = new { user = result.User, tokens = result.Tokens } });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request, ct);
        if (!result.Success)
            return Unauthorized(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, data = new { user = result.User, tokens = result.Tokens } });
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return Ok(new { success = true, message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var result = await _authService.ForgotPasswordAsync(request, baseUrl, ct);
        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(request, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId.Value, request, ct);
        if (!result.Success)
            return BadRequest(new { success = false, error = new { code = result.ErrorCode, message = result.Message } });
        return Ok(new { success = true, message = result.Message });
    }
}

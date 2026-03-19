using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string baseUrl, CancellationToken ct = default);
    Task<AuthResult> VerifyEmailAsync(string token, CancellationToken ct = default);
    Task<AuthResult> ResendVerificationEmailAsync(string email, string baseUrl, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, string? deviceInfo, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<AuthResult> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request, string baseUrl, CancellationToken ct = default);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}

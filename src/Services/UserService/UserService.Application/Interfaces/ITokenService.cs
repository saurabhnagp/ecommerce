using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int AccessTokenExpirySeconds { get; }
    int RefreshTokenExpiryDays { get; }
    Task<string> StoreRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt, string? deviceInfo, string? ipAddress, CancellationToken ct = default);
    Task<Guid?> ValidateAndRevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);
}

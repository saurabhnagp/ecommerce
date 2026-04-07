using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}

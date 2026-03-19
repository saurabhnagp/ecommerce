using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UserDbContext _db;

    public RefreshTokenRepository(UserDbContext db)
    {
        _db = db;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow, ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _db.RefreshTokens.AddAsync(refreshToken, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        _db.RefreshTokens.Update(refreshToken);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _db.RefreshTokens.Where(r => r.UserId == userId && r.RevokedAt == null).ToListAsync(ct);
        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;
        if (tokens.Count > 0)
        {
            _db.RefreshTokens.UpdateRange(tokens);
            await _db.SaveChangesAsync(ct);
        }
    }
}

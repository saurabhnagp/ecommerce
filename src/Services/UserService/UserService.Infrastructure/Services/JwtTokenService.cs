using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AmCart.UserService.Application.Common;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AmCart.UserService.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtTokenService(
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        _issuer = configuration["Jwt:Issuer"] ?? "amcart";
        _audience = configuration["Jwt:Audience"] ?? "amcart-api";
        _accessTokenExpiryMinutes = int.TryParse(configuration["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 15;
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public int AccessTokenExpirySeconds => _accessTokenExpiryMinutes * 60;
    public int RefreshTokenExpiryDays => _refreshTokenExpiryDays;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var name = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrEmpty(name)) name = user.Email;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("email_verified", user.IsEmailVerified.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<string> StoreRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt, string? deviceInfo, string? ipAddress, CancellationToken ct = default)
    {
        var rt = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.AddAsync(rt, ct);
        return tokenHash;
    }

    public async Task<Guid?> ValidateAndRevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = TokenHasher.HashToken(refreshToken);
        var rt = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct);
        if (rt == null) return null;
        var userId = rt.UserId;
        await _refreshTokenRepository.RevokeAsync(rt, ct);
        return userId;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = TokenHasher.HashToken(refreshToken);
        var rt = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct);
        if (rt != null)
            await _refreshTokenRepository.RevokeAsync(rt, ct);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, ct);
    }
}

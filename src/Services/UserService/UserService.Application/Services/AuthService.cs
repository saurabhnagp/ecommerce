using System.Security.Cryptography;
using AmCart.UserService.Application;
using AmCart.UserService.Application.Common;
using AmCart.UserService.Application.Configuration;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AmCart.UserService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IExternalOAuthExchangeService _externalOAuth;
    private readonly IOptions<OAuthProvidersOptions> _oauthOptions;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int EmailVerificationExpiryHours = 24;
    private const int PasswordResetExpiryHours = 1;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IExternalOAuthExchangeService externalOAuth,
        IOptions<OAuthProvidersOptions> oauthOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _externalOAuth = externalOAuth;
        _oauthOptions = oauthOptions;
    }

    public async Task<AuthResult> ExternalLoginAsync(
        ExternalAuthProvider provider,
        ExternalLoginRequest request,
        string? ipAddress,
        string? deviceInfo,
        CancellationToken ct = default)
    {
        var section = provider switch
        {
            ExternalAuthProvider.Google => _oauthOptions.Value.Google,
            ExternalAuthProvider.Facebook => _oauthOptions.Value.Facebook,
            ExternalAuthProvider.Twitter => _oauthOptions.Value.Twitter,
            _ => null
        };

        if (section == null || !section.IsConfigured)
            return AuthResult.Fail("OAUTH_NOT_CONFIGURED", "Social sign-in is not configured.");

        var redirectUri = request.RedirectUri?.Trim() ?? "";
        if (section.RedirectUriAllowlist.Length == 0 ||
            !section.RedirectUriAllowlist.Contains(redirectUri, StringComparer.Ordinal))
            return AuthResult.Fail("OAUTH_INVALID_REDIRECT", "Invalid redirect URI.");

        if (string.IsNullOrWhiteSpace(request.Code))
            return AuthResult.Fail("OAUTH_INVALID_CODE", "Authorization code is missing.");

        var profile = await _externalOAuth.ExchangeAsync(
            provider,
            request.Code.Trim(),
            request.CodeVerifier?.Trim(),
            redirectUri,
            ct);

        if (profile == null)
            return AuthResult.Fail("OAUTH_PROVIDER_ERROR", "Could not complete sign-in with the provider. Try again.");

        var user = provider switch
        {
            ExternalAuthProvider.Google => await _userRepository.GetByGoogleIdAsync(profile.ProviderUserId, ct),
            ExternalAuthProvider.Facebook => await _userRepository.GetByFacebookIdAsync(profile.ProviderUserId, ct),
            ExternalAuthProvider.Twitter => await _userRepository.GetByTwitterIdAsync(profile.ProviderUserId, ct),
            _ => null
        };

        if (user != null)
            return await CompleteSocialLoginAsync(user, ipAddress, deviceInfo, ct);

        var email = profile.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            if (provider == ExternalAuthProvider.Twitter)
                email = $"x_{profile.ProviderUserId}@oauth.amcart.internal";
            else
                return AuthResult.Fail(
                    "OAUTH_EMAIL_REQUIRED",
                    "Your account did not return an email. Grant email permission or use another sign-in method.");
        }

        var existing = await _userRepository.GetByEmailAsync(email, ct);
        if (existing != null && IsEmailPasswordAccount(existing))
            return AuthResult.Fail(
                "EMAIL_USE_PASSWORD",
                "This email is already registered. Please sign in with your email and password.");

        if (existing != null)
            return AuthResult.Fail(
                "EMAIL_IN_USE",
                "This email is already associated with another account.");

        var (firstName, lastName) = ResolveNameParts(profile);
        var authProvider = provider switch
        {
            ExternalAuthProvider.Google => "google",
            ExternalAuthProvider.Facebook => "facebook",
            ExternalAuthProvider.Twitter => "twitter",
            _ => "email"
        };

        user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AuthProvider = authProvider,
            Role = "customer",
            Status = "active",
            IsEmailVerified = true,
            EmailVerifiedAt = DateTime.UtcNow,
            PasswordHash = null,
            GoogleId = provider == ExternalAuthProvider.Google ? profile.ProviderUserId : null,
            FacebookId = provider == ExternalAuthProvider.Facebook ? profile.ProviderUserId : null,
            TwitterId = provider == ExternalAuthProvider.Twitter ? profile.ProviderUserId : null,
            AvatarUrl = profile.PictureUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);
        return await CompleteSocialLoginAsync(user, ipAddress, deviceInfo, ct);
    }

    private static bool IsEmailPasswordAccount(User u) =>
        !string.IsNullOrEmpty(u.PasswordHash) ||
        string.Equals(u.AuthProvider, "email", StringComparison.OrdinalIgnoreCase);

    private static (string FirstName, string LastName) ResolveNameParts(ExternalLoginProfile p)
    {
        if (!string.IsNullOrWhiteSpace(p.GivenName) || !string.IsNullOrWhiteSpace(p.FamilyName))
        {
            var f = (p.GivenName ?? "User").Trim();
            var l = (p.FamilyName ?? "User").Trim();
            return (Clamp(f, 100), Clamp(l, 100));
        }

        var full = (p.FullName ?? "User").Trim();
        if (string.IsNullOrWhiteSpace(full))
            return ("User", "User");

        var parts = full.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
            return (Clamp(parts[0], 100), "User");

        var first = Clamp(parts[0], 100);
        var last = Clamp(string.Join(' ', parts.Skip(1)), 100);
        return (first, last);
    }

    private static string Clamp(string s, int max) =>
        s.Length <= max ? s : s[..max];

    private async Task<AuthResult> CompleteSocialLoginAsync(
        User user,
        string? ipAddress,
        string? deviceInfo,
        CancellationToken ct)
    {
        if (user.Status != "active")
            return AuthResult.Fail("ACCOUNT_DISABLED", "Account is not active.");

        if (!user.IsEmailVerified && string.Equals(user.AuthProvider, "email", StringComparison.OrdinalIgnoreCase))
            return AuthResult.Fail("EMAIL_NOT_VERIFIED", "Please verify your email before logging in.");

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(
            user.Id,
            TokenHasher.HashToken(refreshToken),
            DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiryDays),
            deviceInfo,
            ipAddress,
            ct);

        return AuthResult.Ok(user.ToDto(), new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _tokenService.AccessTokenExpirySeconds
        });
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string baseUrl, CancellationToken ct = default)
    {
        if (request.Password != request.ConfirmPassword)
            return AuthResult.Fail("PASSWORD_MISMATCH", "Password and confirmation do not match.");

        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
            return AuthResult.Fail("INVALID_PASSWORD", passwordError);

        var existing = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (existing != null)
            return AuthResult.Fail("EMAIL_EXISTS", "Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            Gender = request.Gender,
            AuthProvider = "email",
            Role = "customer",
            Status = "pending",
            IsEmailVerified = false,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailVerificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(EmailVerificationExpiryHours),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);

        var verificationLink = $"{baseUrl?.TrimEnd('/')}/auth/verify-email?token={user.EmailVerificationToken}";
        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, verificationLink, ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, TokenHasher.HashToken(refreshToken), DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiryDays), null, null, ct);

        return AuthResult.Ok(
            user.ToDto(),
            new TokenResponse { AccessToken = accessToken, RefreshToken = refreshToken, ExpiresIn = _tokenService.AccessTokenExpirySeconds },
            "Registration successful. Please verify your email.");
    }

    public async Task<AuthResult> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(token, ct);
        if (user == null)
            return AuthResult.Fail("INVALID_TOKEN", "Invalid verification token.");
        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return AuthResult.Fail("TOKEN_EXPIRED", "Verification token has expired.");
        if (user.IsEmailVerified)
            return AuthResult.OkWithMessage("Email already verified.");

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.Status = "active";
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, ct);
        return AuthResult.OkWithMessage("Email verified successfully. You can now login.");
    }

    public async Task<AuthResult> ResendVerificationEmailAsync(string email, string baseUrl, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant(), ct);
        if (user == null || user.IsEmailVerified)
            return AuthResult.OkWithMessage("If the email exists and is unverified, a verification link has been sent.");

        user.EmailVerificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(EmailVerificationExpiryHours);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        var verificationLink = $"{baseUrl?.TrimEnd('/')}/auth/verify-email?token={user.EmailVerificationToken}";
        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, verificationLink, ct);

        return AuthResult.OkWithMessage("If the email exists and is unverified, a verification link has been sent.");
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, string? deviceInfo, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user == null)
            return AuthResult.Fail("INVALID_CREDENTIALS", "Invalid email or password.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return AuthResult.Fail("ACCOUNT_LOCKED", "Account is temporarily locked. Try again later.");

        // BUG FIX: Reset failed attempts when lockout has expired
        if (user.LockoutEnd.HasValue && user.LockoutEnd <= DateTime.UtcNow)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
        }

        if (user.PasswordHash == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, ct);
            return AuthResult.Fail("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        if (!user.IsEmailVerified)
            return AuthResult.Fail("EMAIL_NOT_VERIFIED", "Please verify your email before logging in.");

        if (user.Status != "active")
            return AuthResult.Fail("ACCOUNT_DISABLED", "Account is not active.");

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiryDays = request.RememberMe ? _tokenService.RefreshTokenExpiryDays * 2 : _tokenService.RefreshTokenExpiryDays;
        await _tokenService.StoreRefreshTokenAsync(user.Id, TokenHasher.HashToken(refreshToken), DateTime.UtcNow.AddDays(expiryDays), deviceInfo, ipAddress, ct);

        return AuthResult.Ok(user.ToDto(), new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _tokenService.AccessTokenExpirySeconds
        });
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var userId = await _tokenService.ValidateAndRevokeRefreshTokenAsync(request.RefreshToken, ct);
        if (userId == null)
            return AuthResult.Fail("INVALID_REFRESH_TOKEN", "Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user == null || user.Status != "active")
            return AuthResult.Fail("USER_NOT_FOUND", "User not found or inactive.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, TokenHasher.HashToken(refreshToken), DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiryDays), null, null, ct);

        return AuthResult.Ok(user.ToDto(), new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _tokenService.AccessTokenExpirySeconds
        });
    }

    public async Task<AuthResult> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ct);
        return AuthResult.OkWithMessage("Logged out successfully.");
    }

    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request, string baseUrl, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user == null)
            return AuthResult.OkWithMessage("If the email exists, a password reset link has been sent.");

        user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(PasswordResetExpiryHours);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        var resetLink = $"{baseUrl?.TrimEnd('/')}/auth/reset-password?token={user.PasswordResetToken}";
        await _emailService.SendPasswordResetAsync(user.Email, user.FirstName, resetLink, ct);

        return AuthResult.OkWithMessage("If the email exists, a password reset link has been sent.");
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (request.Password != request.ConfirmPassword)
            return AuthResult.Fail("PASSWORD_MISMATCH", "Password and confirmation do not match.");

        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
            return AuthResult.Fail("INVALID_PASSWORD", passwordError);

        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, ct);
        if (user == null)
            return AuthResult.Fail("INVALID_TOKEN", "Invalid reset token.");
        if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return AuthResult.Fail("TOKEN_EXPIRED", "Reset token has expired.");

        user.PasswordHash = _passwordHasher.Hash(request.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        // Security: invalidate all existing sessions after password reset
        await _tokenService.RevokeAllUserTokensAsync(user.Id, ct);

        return AuthResult.OkWithMessage("Password has been reset successfully.");
    }

    public async Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return AuthResult.Fail("PASSWORD_MISMATCH", "New password and confirmation do not match.");

        var passwordError = ValidatePassword(request.NewPassword);
        if (passwordError != null)
            return AuthResult.Fail("INVALID_PASSWORD", passwordError);

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return AuthResult.Fail("USER_NOT_FOUND", "User not found.");

        if (user.PasswordHash == null || !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return AuthResult.Fail("INVALID_CREDENTIALS", "Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        await _tokenService.RevokeAllUserTokensAsync(user.Id, ct);

        return AuthResult.OkWithMessage("Password changed successfully. Please log in again.");
    }

    internal static string? ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return "Password must be at least 8 characters.";
        if (!password.Any(char.IsUpper))
            return "Password must contain at least one uppercase letter.";
        if (!password.Any(char.IsLower))
            return "Password must contain at least one lowercase letter.";
        if (!password.Any(char.IsDigit))
            return "Password must contain at least one number.";
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return "Password must contain at least one special character.";
        return null;
    }
}

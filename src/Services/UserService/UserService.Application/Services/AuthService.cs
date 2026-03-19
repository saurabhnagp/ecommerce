using System.Security.Cryptography;
using AmCart.UserService.Application.Common;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int EmailVerificationExpiryHours = 24;
    private const int PasswordResetExpiryHours = 1;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string baseUrl, CancellationToken ct = default)
    {
        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
            return AuthResult.Fail("INVALID_PASSWORD", passwordError);

        var existing = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (existing != null)
            return AuthResult.Fail("EMAIL_EXISTS", "Email already registered.");

        var nameParts = request.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : request.Email;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
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

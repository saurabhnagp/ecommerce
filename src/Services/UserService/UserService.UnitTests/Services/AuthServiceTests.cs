using AmCart.UserService.Application.Configuration;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Application.Services;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.UnitTests.TestFixtures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AmCart.UserService.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IEmailService> _emailService;
    private readonly Mock<IExternalOAuthExchangeService> _externalOAuth;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _tokenService = new Mock<ITokenService>();
        _emailService = new Mock<IEmailService>();
        _externalOAuth = new Mock<IExternalOAuthExchangeService>();

        _tokenService.Setup(t => t.AccessTokenExpirySeconds).Returns(900);
        _tokenService.Setup(t => t.RefreshTokenExpiryDays).Returns(7);
        _tokenService.Setup(t => t.StoreRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync("stored");
        _tokenService.Setup(t => t.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var oauthOpts = Options.Create(new OAuthProvidersOptions());
        _sut = new AuthService(
            _userRepo.Object,
            _passwordHasher.Object,
            _tokenService.Object,
            _emailService.Object,
            _externalOAuth.Object,
            oauthOpts);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccessWithUserAndTokens()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var request = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "new@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1",
            Phone = "+1234567890",
            Gender = "female"
        };

        var result = await _sut.RegisterAsync(request, "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Tokens);
        Assert.Equal("new@example.com", result.User.Email);
        Assert.Equal("Jane Doe", result.User.Name);
        Assert.False(result.User.IsVerified);
        Assert.Equal("access", result.Tokens.AccessToken);
        Assert.Equal("refresh", result.Tokens.RefreshToken);
        Assert.Equal("Registration successful. Please verify your email.", result.Message);
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new@example.com" && u.FirstName == "Jane" && u.LastName == "Doe"), It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationAsync("new@example.com", "Jane", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailure()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserFixtures.CreateActiveVerifiedUser());

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "existing@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1",
            Phone = "+1000000000"
        };

        var result = await _sut.RegisterAsync(request, "https://app.com", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("EMAIL_EXISTS", result.ErrorCode);
        Assert.Equal("Email already registered.", result.Message);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("short", "Password must be at least 8 characters.")]
    [InlineData("nouppercase1!", "Password must contain at least one uppercase letter.")]
    [InlineData("NOLOWERCASE1!", "Password must contain at least one lowercase letter.")]
    [InlineData("NoNumbers!!", "Password must contain at least one number.")]
    [InlineData("NoSpecial123", "Password must contain at least one special character.")]
    public async Task RegisterAsync_InvalidPassword_ReturnsFailure(string password, string expectedMessage)
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "new@example.com",
            Password = password,
            ConfirmPassword = password,
            Phone = "+1000000000"
        };

        var result = await _sut.RegisterAsync(request, "https://app.com", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_PASSWORD", result.ErrorCode);
        Assert.Equal(expectedMessage, result.Message);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_SingleWordName_SplitsCorrectly()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var request = new RegisterRequest
        {
            FirstName = "OnlyFirst",
            LastName = "Surname",
            Email = "a@b.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1",
            Phone = "+1000000001"
        };

        var result = await _sut.RegisterAsync(request, "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.FirstName == "OnlyFirst" && u.LastName == "Surname"), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region VerifyEmailAsync

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_ReturnsSuccessAndUpdatesUser()
    {
        var user = UserFixtures.CreateUnverifiedUser();
        _userRepo.Setup(r => r.GetByEmailVerificationTokenAsync("validtoken", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync("validtoken", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Email verified successfully. You can now login.", result.Message);
        Assert.True(user.IsEmailVerified);
        Assert.Equal("active", user.Status);
        Assert.Null(user.EmailVerificationToken);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendWelcomeEmailAsync(user.Email, user.FirstName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFailure()
    {
        _userRepo.Setup(r => r.GetByEmailVerificationTokenAsync("invalid", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.VerifyEmailAsync("invalid", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_TOKEN", result.ErrorCode);
        Assert.Equal("Invalid verification token.", result.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredToken_ReturnsFailure()
    {
        var user = UserFixtures.CreateUserWithExpiredVerificationToken();
        _userRepo.Setup(r => r.GetByEmailVerificationTokenAsync("expired", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync("expired", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("TOKEN_EXPIRED", result.ErrorCode);
        Assert.Equal("Verification token has expired.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyVerified_ReturnsSuccessWithMessage()
    {
        var user = UserFixtures.CreateAlreadyVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailVerificationTokenAsync("alreadydone", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync("alreadydone", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Email already verified.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ResendVerificationEmailAsync

    [Fact]
    public async Task ResendVerificationEmailAsync_UnverifiedUser_SendsNewEmail()
    {
        var user = UserFixtures.CreateUnverifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ResendVerificationEmailAsync("user@example.com", "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationAsync(user.Email, user.FirstName, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_AlreadyVerified_ReturnsSameMessageNoEmail()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ResendVerificationEmailAsync("user@example.com", "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        _emailService.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserNotFound_ReturnsSameMessage()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.ResendVerificationEmailAsync("nobody@example.com", "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        _emailService.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithTokens()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("password123", "hashed")).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var request = new LoginRequest { Email = "user@example.com", Password = "password123", RememberMe = false };

        var result = await _sut.LoginAsync(request, "1.2.3.4", "device", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Tokens);
        Assert.Equal("access", result.Tokens.AccessToken);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsInvalidCredentials()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nobody@example.com", Password = "AnyP@ss1" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_CREDENTIALS", result.ErrorCode);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_IncrementsFailedAttemptsAndReturnsFailure()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = new LoginRequest { Email = "user@example.com", Password = "wrong" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_CREDENTIALS", result.ErrorCode);
        Assert.Equal(1, user.FailedLoginAttempts);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_FiveFailedAttempts_SetsLockout()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        user.FailedLoginAttempts = 4;
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = new LoginRequest { Email = "user@example.com", Password = "wrong" };

        await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.NotNull(user.LockoutEnd);
    }

    [Fact]
    public async Task LoginAsync_ExpiredLockout_ResetsFailedAttempts()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        user.FailedLoginAttempts = 5;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1);
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("password123", "hashed")).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsFailure()
    {
        var user = UserFixtures.CreateLockedOutUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ACCOUNT_LOCKED", result.ErrorCode);
        Assert.Equal("Account is temporarily locked. Try again later.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_EmailNotVerified_ReturnsFailure()
    {
        var user = UserFixtures.CreateUnverifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var request = new LoginRequest { Email = "user@example.com", Password = "password" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("EMAIL_NOT_VERIFIED", result.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_AccountSuspended_ReturnsFailure()
    {
        var user = UserFixtures.CreateSuspendedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var request = new LoginRequest { Email = "user@example.com", Password = "password" };

        var result = await _sut.LoginAsync(request, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ACCOUNT_DISABLED", result.ErrorCode);
    }

    #endregion

    #region RefreshTokenAsync

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        var userId = Guid.NewGuid();
        var user = UserFixtures.CreateActiveVerifiedUser(userId);
        _tokenService.Setup(t => t.ValidateAndRevokeRefreshTokenAsync("validrefresh", It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("newaccess");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("newrefresh");

        var request = new RefreshTokenRequest { RefreshToken = "validrefresh" };

        var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Tokens);
        Assert.Equal("newaccess", result.Tokens.AccessToken);
        Assert.Equal("newrefresh", result.Tokens.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsFailure()
    {
        _tokenService.Setup(t => t.ValidateAndRevokeRefreshTokenAsync("invalid", It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        var request = new RefreshTokenRequest { RefreshToken = "invalid" };

        var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
        _userRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        _tokenService.Setup(t => t.ValidateAndRevokeRefreshTokenAsync("valid", It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "valid" }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("USER_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserInactive_ReturnsFailure()
    {
        var user = UserFixtures.CreateSuspendedUser();
        _tokenService.Setup(t => t.ValidateAndRevokeRefreshTokenAsync("valid", It.IsAny<CancellationToken>())).ReturnsAsync(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "valid" }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("USER_NOT_FOUND", result.ErrorCode);
    }

    #endregion

    #region LogoutAsync

    [Fact]
    public async Task LogoutAsync_Always_ReturnsSuccessAndRevokesToken()
    {
        var result = await _sut.LogoutAsync("anytoken", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Logged out successfully.", result.Message);
        _tokenService.Verify(t => t.RevokeRefreshTokenAsync("anytoken", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ForgotPasswordAsync

    [Fact]
    public async Task ForgotPasswordAsync_UserExists_SetsResetTokenAndSendsEmail()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var request = new ForgotPasswordRequest { Email = "user@example.com" };

        var result = await _sut.ForgotPasswordAsync(request, "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiresAt);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendPasswordResetAsync("user@example.com", "John", It.Is<string>(s => s.Contains(user.PasswordResetToken!)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserDoesNotExist_ReturnsSameMessageNoException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var request = new ForgotPasswordRequest { Email = "nobody@example.com" };

        var result = await _sut.ForgotPasswordAsync(request, "https://app.com", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("If the email exists, a password reset link has been sent.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync

    [Fact]
    public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordAndRevokesAllTokens()
    {
        var user = UserFixtures.CreateUserWithPasswordResetToken();
        _userRepo.Setup(r => r.GetByPasswordResetTokenAsync("resettoken123", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Hash("NewSecureP@ss1")).Returns("newhash");

        var request = new ResetPasswordRequest
        {
            Token = "resettoken123",
            Password = "NewSecureP@ss1",
            ConfirmPassword = "NewSecureP@ss1"
        };

        var result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("newhash", user.PasswordHash);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetTokenExpiresAt);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.Equal("Password has been reset successfully.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(t => t.RevokeAllUserTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_PasswordMismatch_ReturnsFailure()
    {
        var request = new ResetPasswordRequest
        {
            Token = "token",
            Password = "NewSecureP@ss1",
            ConfirmPassword = "DifferentP@ss1"
        };

        var result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("PASSWORD_MISMATCH", result.ErrorCode);
        _userRepo.Verify(r => r.GetByPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidPassword_ReturnsFailure()
    {
        var request = new ResetPasswordRequest
        {
            Token = "token",
            Password = "weak",
            ConfirmPassword = "weak"
        };

        var result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_PASSWORD", result.ErrorCode);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFailure()
    {
        _userRepo.Setup(r => r.GetByPasswordResetTokenAsync("invalid", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var request = new ResetPasswordRequest
        {
            Token = "invalid",
            Password = "NewSecureP@ss1",
            ConfirmPassword = "NewSecureP@ss1"
        };

        var result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_TOKEN", result.ErrorCode);
        Assert.Equal("Invalid reset token.", result.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsFailure()
    {
        var user = UserFixtures.CreateUserWithExpiredPasswordResetToken();
        _userRepo.Setup(r => r.GetByPasswordResetTokenAsync("expired", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var request = new ResetPasswordRequest
        {
            Token = "expired",
            Password = "NewSecureP@ss1",
            ConfirmPassword = "NewSecureP@ss1"
        };

        var result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("TOKEN_EXPIRED", result.ErrorCode);
        Assert.Equal("Reset token has expired.", result.Message);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_ChangesPasswordAndRevokesTokens()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("OldP@ssword1", "hashed")).Returns(true);
        _passwordHasher.Setup(h => h.Hash("NewSecureP@ss1")).Returns("newhash");

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ssword1",
            NewPassword = "NewSecureP@ss1",
            ConfirmNewPassword = "NewSecureP@ss1"
        };

        var result = await _sut.ChangePasswordAsync(user.Id, request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("newhash", user.PasswordHash);
        _tokenService.Verify(t => t.RevokeAllUserTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFailure()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "NewSecureP@ss1",
            ConfirmNewPassword = "NewSecureP@ss1"
        };

        var result = await _sut.ChangePasswordAsync(user.Id, request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_CREDENTIALS", result.ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordMismatch_ReturnsFailure()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ss1",
            NewPassword = "NewSecureP@ss1",
            ConfirmNewPassword = "DifferentP@ss1"
        };

        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("PASSWORD_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WeakNewPassword_ReturnsFailure()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ss1",
            NewPassword = "weak",
            ConfirmNewPassword = "weak"
        };

        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_PASSWORD", result.ErrorCode);
    }

    #endregion
}

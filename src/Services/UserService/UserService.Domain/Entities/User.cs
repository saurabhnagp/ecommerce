namespace AmCart.UserService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    /// <summary>Cognito subject ID when using Cognito; null for custom auth.</summary>
    public string? CognitoSub { get; set; }

    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public bool PhoneVerified { get; set; }

    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    public string AuthProvider { get; set; } = "email";
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    /// <summary>X (Twitter) OAuth 2.0 user id (string).</summary>
    public string? TwitterId { get; set; }

    public string Role { get; set; } = "customer";
    /// <summary>JSON array of permission strings, e.g. ["manage_products"].</summary>
    public string? PermissionsJson { get; set; }

    public string Status { get; set; } = "active";
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>JSON-serialized preferences (language, currency, timezone, theme).</summary>
    public string? PreferencesJson { get; set; }
    /// <summary>JSON-serialized metadata (referral_code, etc.).</summary>
    public string? MetadataJson { get; set; }

    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int LoyaltyPoints { get; set; }

    // Custom auth (when not using Cognito)
    public string? PasswordHash { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public NotificationPreferences? NotificationPreferences { get; set; }
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

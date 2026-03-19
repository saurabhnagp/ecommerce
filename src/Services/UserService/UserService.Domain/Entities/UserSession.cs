namespace AmCart.UserService.Domain.Entities;

public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string? CognitoSessionId { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public User User { get; set; } = null!;
}

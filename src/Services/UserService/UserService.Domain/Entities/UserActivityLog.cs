namespace AmCart.UserService.Domain.Entities;

public class UserActivityLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }

    public string ActivityType { get; set; } = null!;
    public string? ActivityDescription { get; set; }
    /// <summary>JSON-serialized details (order_id, old_value, new_value, etc.).</summary>
    public string? DetailsJson { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }

    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}

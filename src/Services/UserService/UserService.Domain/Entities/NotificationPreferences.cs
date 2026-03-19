namespace AmCart.UserService.Domain.Entities;

public class NotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public bool EmailOrderUpdates { get; set; } = true;
    public bool EmailShippingUpdates { get; set; } = true;
    public bool EmailPromotions { get; set; } = true;
    public bool EmailNewsletters { get; set; }
    public bool EmailPriceDrops { get; set; } = true;
    public bool EmailBackInStock { get; set; } = true;

    public bool SmsOrderUpdates { get; set; } = true;
    public bool SmsDeliveryUpdates { get; set; } = true;
    public bool SmsPromotions { get; set; }
    public bool SmsOtp { get; set; } = true;

    public bool PushEnabled { get; set; } = true;
    public bool PushOrderUpdates { get; set; } = true;
    public bool PushPromotions { get; set; } = true;
    public bool PushChatMessages { get; set; } = true;

    public bool WhatsappEnabled { get; set; }
    public bool WhatsappOrderUpdates { get; set; }

    public string PromotionFrequency { get; set; } = "weekly";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}

namespace AmCart.UserService.Domain.Entities;

public class NewsletterSubscription
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime SubscribedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
}

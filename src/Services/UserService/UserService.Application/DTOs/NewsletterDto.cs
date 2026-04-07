using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class NewsletterSubscriptionDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime SubscribedAt { get; set; }
}

public class SubscribeNewsletterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;
}

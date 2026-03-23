using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface INewsletterService
{
    Task<IReadOnlyList<NewsletterSubscriptionDto>> GetAllAsync(CancellationToken ct = default);
    Task<NewsletterSubscriptionDto> SubscribeAsync(string email, CancellationToken ct = default);
    Task<bool> UnsubscribeAsync(string email, CancellationToken ct = default);
}

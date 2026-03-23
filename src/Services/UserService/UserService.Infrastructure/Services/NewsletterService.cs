using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Services;

public class NewsletterService : INewsletterService
{
    private readonly UserDbContext _db;

    public NewsletterService(UserDbContext db) => _db = db;

    public async Task<IReadOnlyList<NewsletterSubscriptionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _db.NewsletterSubscriptions
            .OrderByDescending(n => n.SubscribedAt)
            .ToListAsync(ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<NewsletterSubscriptionDto> SubscribeAsync(string email, CancellationToken ct = default)
    {
        var normalised = email.Trim().ToLowerInvariant();
        var existing = await _db.NewsletterSubscriptions
            .FirstOrDefaultAsync(n => n.Email == normalised, ct);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                await _db.SaveChangesAsync(ct);
            }
            return ToDto(existing);
        }

        var entity = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = normalised,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        };
        _db.NewsletterSubscriptions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<bool> UnsubscribeAsync(string email, CancellationToken ct = default)
    {
        var normalised = email.Trim().ToLowerInvariant();
        var entity = await _db.NewsletterSubscriptions
            .FirstOrDefaultAsync(n => n.Email == normalised, ct);
        if (entity == null || !entity.IsActive) return false;

        entity.IsActive = false;
        entity.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static NewsletterSubscriptionDto ToDto(NewsletterSubscription n) => new()
    {
        Id = n.Id,
        Email = n.Email,
        IsActive = n.IsActive,
        SubscribedAt = n.SubscribedAt
    };
}

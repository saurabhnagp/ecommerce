using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly UserDbContext _db;

    public ContactMessageService(UserDbContext db) => _db = db;

    public async Task<ContactMessageDto> SubmitAsync(CreateContactMessageRequest request, CancellationToken ct = default)
    {
        var entity = new ContactMessage
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Subject = request.Subject.Trim(),
            Comment = request.Comment.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.ContactMessages.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<IEnumerable<ContactMessageDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => ToDto(m))
            .ToListAsync(ct);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.ContactMessages.FindAsync(new object[] { id }, ct);
        if (entity == null) return false;
        entity.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ContactMessageDto ToDto(ContactMessage m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Email = m.Email,
        Subject = m.Subject,
        Comment = m.Comment,
        IsRead = m.IsRead,
        CreatedAt = m.CreatedAt
    };
}

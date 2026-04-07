using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Services;

public class TestimonialService : ITestimonialService
{
    private readonly UserDbContext _db;

    public TestimonialService(UserDbContext db) => _db = db;

    public async Task<IEnumerable<TestimonialDto>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _db.Testimonials
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => ToDto(t))
            .ToListAsync(ct);
    }

    public async Task<TestimonialDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await _db.Testimonials.FindAsync(new object[] { id }, ct);
        return t == null ? null : ToDto(t);
    }

    public async Task<TestimonialDto> CreateAsync(CreateTestimonialRequest request, CancellationToken ct = default)
    {
        var entity = new Testimonial
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            PhotoUrl = request.PhotoUrl,
            Comment = request.Comment,
            Rating = request.Rating,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Testimonials.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<TestimonialDto?> UpdateAsync(Guid id, UpdateTestimonialRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Testimonials.FindAsync(new object[] { id }, ct);
        if (entity == null) return null;

        if (request.CustomerName != null) entity.CustomerName = request.CustomerName;
        if (request.PhotoUrl != null) entity.PhotoUrl = request.PhotoUrl;
        if (request.Comment != null) entity.Comment = request.Comment;
        if (request.Rating.HasValue) entity.Rating = request.Rating.Value;
        if (request.SortOrder.HasValue) entity.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Testimonials.FindAsync(new object[] { id }, ct);
        if (entity == null) return false;
        _db.Testimonials.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static TestimonialDto ToDto(Testimonial t) => new()
    {
        Id = t.Id,
        CustomerName = t.CustomerName,
        PhotoUrl = t.PhotoUrl,
        Comment = t.Comment,
        Rating = t.Rating,
        SortOrder = t.SortOrder
    };
}

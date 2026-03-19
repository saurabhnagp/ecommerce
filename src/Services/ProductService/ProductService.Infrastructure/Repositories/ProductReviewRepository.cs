using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class ProductReviewRepository : IProductReviewRepository
{
    private readonly ProductDbContext _db;

    public ProductReviewRepository(ProductDbContext db) => _db = db;

    public async Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<(IReadOnlyList<ProductReview> Items, int TotalCount)> GetByProductIdAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<ProductReview?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct = default)
    {
        return await _db.ProductReviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, ct);
    }

    public async Task<double> GetAverageRatingAsync(Guid productId, CancellationToken ct = default)
    {
        var hasReviews = await _db.ProductReviews.AnyAsync(r => r.ProductId == productId && r.IsApproved, ct);
        if (!hasReviews) return 0;
        return await _db.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AverageAsync(r => r.Rating, ct);
    }

    public async Task<int> GetReviewCountAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.ProductReviews.CountAsync(r => r.ProductId == productId && r.IsApproved, ct);
    }

    public async Task AddAsync(ProductReview review, CancellationToken ct = default)
    {
        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProductReview review, CancellationToken ct = default)
    {
        _db.ProductReviews.Update(review);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ProductReview review, CancellationToken ct = default)
    {
        _db.ProductReviews.Remove(review);
        await _db.SaveChangesAsync(ct);
    }
}

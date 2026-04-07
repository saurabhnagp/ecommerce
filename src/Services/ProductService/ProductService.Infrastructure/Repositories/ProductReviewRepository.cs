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

    public async Task<IReadOnlyDictionary<Guid, bool>> GetUserVotesForReviewIdsAsync(
        IReadOnlyList<Guid> reviewIds,
        Guid userId,
        CancellationToken ct = default)
    {
        if (reviewIds.Count == 0)
            return new Dictionary<Guid, bool>();

        return await _db.ProductReviewVotes
            .AsNoTracking()
            .Where(v => v.UserId == userId && reviewIds.Contains(v.ReviewId))
            .ToDictionaryAsync(v => v.ReviewId, v => v.IsUp, ct);
    }

    public async Task ApplyVoteToggleAsync(Guid reviewId, Guid userId, bool wantUp, CancellationToken ct = default)
    {
        var existing = await _db.ProductReviewVotes
            .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId, ct);

        if (existing == null)
        {
            _db.ProductReviewVotes.Add(new ProductReviewVote
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = userId,
                IsUp = wantUp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }
        else if (existing.IsUp == wantUp)
        {
            _db.ProductReviewVotes.Remove(existing);
        }
        else
        {
            existing.IsUp = wantUp;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        await RefreshReviewVoteCountsAsync(reviewId, ct);
    }

    public async Task RefreshReviewVoteCountsAsync(Guid reviewId, CancellationToken ct = default)
    {
        var up = await _db.ProductReviewVotes.CountAsync(v => v.ReviewId == reviewId && v.IsUp, ct);
        var down = await _db.ProductReviewVotes.CountAsync(v => v.ReviewId == reviewId && !v.IsUp, ct);
        var r = await _db.ProductReviews.FirstOrDefaultAsync(x => x.Id == reviewId, ct);
        if (r == null) return;
        r.HelpfulCount = up;
        r.NotHelpfulCount = down;
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}

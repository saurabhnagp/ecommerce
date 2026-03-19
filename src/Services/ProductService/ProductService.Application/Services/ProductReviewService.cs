using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepository _reviews;
    private readonly IProductRepository _products;

    public ProductReviewService(IProductReviewRepository reviews, IProductRepository products)
    {
        _reviews = reviews;
        _products = products;
    }

    public async Task<ProductReviewDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _reviews.GetByIdAsync(id, ct);
        return r?.ToDto();
    }

    public async Task<PagedResult<ProductReviewDto>> GetByProductIdAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _reviews.GetByProductIdAsync(productId, page, pageSize, ct);
        return new PagedResult<ProductReviewDto>
        {
            Items = items.Select(x => x.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductReviewDto?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct = default)
    {
        var r = await _reviews.GetByProductAndUserAsync(productId, userId, ct);
        return r?.ToDto();
    }

    public async Task<ProductReviewDto> CreateAsync(Guid productId, CreateProductReviewRequest request, CancellationToken ct = default)
    {
        var existing = await _reviews.GetByProductAndUserAsync(productId, request.UserId, ct);
        if (existing != null)
            throw new InvalidOperationException("You have already submitted a review for this product.");

        var review = new ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = request.UserId,
            Rating = request.Rating,
            Title = request.Title?.Trim(),
            Comment = request.Comment?.Trim(),
            IsVerifiedPurchase = request.IsVerifiedPurchase,
            IsApproved = false,
            HelpfulCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reviews.AddAsync(review, ct);
        await RecalculateProductRatingAsync(productId, ct);
        var loaded = await _reviews.GetByIdAsync(review.Id, ct);
        return loaded!.ToDto();
    }

    public async Task<ProductReviewDto?> UpdateAsync(Guid id, int? rating, string? title, string? comment, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct);
        if (review == null) return null;

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5) review.Rating = rating.Value;
        if (title != null) review.Title = title.Trim();
        if (comment != null) review.Comment = comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _reviews.UpdateAsync(review, ct);
        await RecalculateProductRatingAsync(review.ProductId, ct);
        var loaded = await _reviews.GetByIdAsync(id, ct);
        return loaded?.ToDto();
    }

    public async Task<bool> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct);
        if (review == null) return false;

        review.IsApproved = true;
        review.UpdatedAt = DateTime.UtcNow;
        await _reviews.UpdateAsync(review, ct);
        await RecalculateProductRatingAsync(review.ProductId, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct);
        if (review == null) return false;

        var productId = review.ProductId;
        await _reviews.DeleteAsync(review, ct);
        await RecalculateProductRatingAsync(productId, ct);
        return true;
    }

    private async Task RecalculateProductRatingAsync(Guid productId, CancellationToken ct)
    {
        var avg = await _reviews.GetAverageRatingAsync(productId, ct);
        var count = await _reviews.GetReviewCountAsync(productId, ct);
        await _products.UpdateRatingAndReviewCountAsync(productId, avg, count, ct);
    }
}

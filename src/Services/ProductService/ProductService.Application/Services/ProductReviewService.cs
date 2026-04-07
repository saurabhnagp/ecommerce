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

    public async Task<PagedResult<ProductReviewDto>> GetByProductIdAsync(
        Guid productId,
        int page,
        int pageSize,
        Guid? currentUserId,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _reviews.GetByProductIdAsync(productId, page, pageSize, ct);
        IReadOnlyDictionary<Guid, bool>? voteMap = null;
        if (currentUserId.HasValue && items.Count > 0)
        {
            voteMap = await _reviews.GetUserVotesForReviewIdsAsync(
                items.Select(x => x.Id).ToList(),
                currentUserId.Value,
                ct);
        }

        var dtos = items.Select(r =>
        {
            bool? my = null;
            if (voteMap != null && voteMap.TryGetValue(r.Id, out var isUp))
                my = isUp;
            return r.ToDto(my);
        }).ToList();

        return new PagedResult<ProductReviewDto>
        {
            Items = dtos,
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

    public async Task<ProductReviewDto> CreateAsync(
        Guid productId,
        Guid userId,
        string? reviewerDisplayName,
        CreateProductReviewRequest request,
        CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, publicOnly: true, ct);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        var existing = await _reviews.GetByProductAndUserAsync(productId, userId, ct);
        if (existing != null)
            throw new InvalidOperationException("You have already submitted a review for this product.");

        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Please add a title or comment to your review.");

        var name = string.IsNullOrWhiteSpace(reviewerDisplayName)
            ? "Customer"
            : reviewerDisplayName.Trim();
        var photo = string.IsNullOrWhiteSpace(request.ReviewerPhotoUrl)
            ? null
            : request.ReviewerPhotoUrl.Trim();

        var review = new ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title?.Trim(),
            Comment = request.Comment?.Trim(),
            IsVerifiedPurchase = request.IsVerifiedPurchase,
            IsApproved = true,
            HelpfulCount = 0,
            NotHelpfulCount = 0,
            ReviewerDisplayName = name,
            ReviewerPhotoUrl = photo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reviews.AddAsync(review, ct);
        await RecalculateProductRatingAsync(productId, ct);
        var loaded = await _reviews.GetByIdAsync(review.Id, ct);
        return loaded!.ToDto();
    }

    public async Task<ProductReviewDto?> VoteAsync(
        Guid productId,
        Guid reviewId,
        Guid userId,
        ReviewVoteRequest request,
        CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(reviewId, ct);
        if (review == null || review.ProductId != productId || !review.IsApproved)
            return null;

        var act = request.Action.Trim().ToLowerInvariant();
        if (act is not ("like" or "dislike"))
            throw new InvalidOperationException("Action must be 'like' or 'dislike'.");

        var wantUp = act == "like";
        await _reviews.ApplyVoteToggleAsync(reviewId, userId, wantUp, ct);
        var loaded = await _reviews.GetByIdAsync(reviewId, ct);
        if (loaded == null) return null;

        var myVote = await _reviews.GetUserVotesForReviewIdsAsync(
            new[] { reviewId },
            userId,
            ct);
        bool? mv = myVote.TryGetValue(reviewId, out var u) ? u : null;
        return loaded.ToDto(mv);
    }

    public async Task<ProductReviewDto?> UpdateAsync(Guid id, Guid userId, bool isAdmin, int? rating, string? title, string? comment, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct);
        if (review == null) return null;
        if (!isAdmin && review.UserId != userId)
            throw new InvalidOperationException("You can only edit your own review.");

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

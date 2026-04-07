using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IProductReviewService
{
    Task<ProductReviewDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductReviewDto>> GetByProductIdAsync(
        Guid productId,
        int page,
        int pageSize,
        Guid? currentUserId,
        CancellationToken ct = default);
    Task<ProductReviewDto?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct = default);
    Task<ProductReviewDto> CreateAsync(
        Guid productId,
        Guid userId,
        string? reviewerDisplayName,
        CreateProductReviewRequest request,
        CancellationToken ct = default);
    Task<ProductReviewDto?> VoteAsync(Guid productId, Guid reviewId, Guid userId, ReviewVoteRequest request, CancellationToken ct = default);
    Task<ProductReviewDto?> UpdateAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        int? rating,
        string? title,
        string? comment,
        CancellationToken ct = default);
    Task<bool> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

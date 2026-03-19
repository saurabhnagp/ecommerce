using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IProductReviewService
{
    Task<ProductReviewDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductReviewDto>> GetByProductIdAsync(Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductReviewDto?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct = default);
    Task<ProductReviewDto> CreateAsync(Guid productId, CreateProductReviewRequest request, CancellationToken ct = default);
    Task<ProductReviewDto?> UpdateAsync(Guid id, int? rating, string? title, string? comment, CancellationToken ct = default);
    Task<bool> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

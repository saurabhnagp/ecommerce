using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface IProductReviewRepository
{
    Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductReview> Items, int TotalCount)> GetByProductIdAsync(Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductReview?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct = default);
    Task<double> GetAverageRatingAsync(Guid productId, CancellationToken ct = default);
    Task<int> GetReviewCountAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(ProductReview review, CancellationToken ct = default);
    Task UpdateAsync(ProductReview review, CancellationToken ct = default);
    Task DeleteAsync(ProductReview review, CancellationToken ct = default);
}

using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IWishlistService
{
    Task<IReadOnlyList<WishlistItemDto>> GetItemsAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetProductIdsAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default);

    Task<bool> RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);

    Task<bool> ContainsAsync(Guid userId, Guid productId, CancellationToken ct = default);
}

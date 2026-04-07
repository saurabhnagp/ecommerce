using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface IWishlistRepository
{
    Task<IReadOnlyList<WishlistItem>> GetItemsForUserAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetProductIdsForUserAsync(Guid userId, CancellationToken ct = default);

    Task<bool> AddItemAsync(Guid userId, Guid productId, CancellationToken ct = default);

    Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default);

    Task<bool> ContainsAsync(Guid userId, Guid productId, CancellationToken ct = default);
}

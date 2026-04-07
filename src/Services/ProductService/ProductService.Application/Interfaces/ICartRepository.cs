using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);

    /// <summary>Guest cart with items (for merge).</summary>
    Task<Cart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken ct = default);

    Task<Cart> GetOrCreateAsync(Guid? userId, string? sessionId, CancellationToken ct = default);

    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken ct = default);

    Task<IReadOnlyList<CartItem>> ListItemsWithProductsAsync(Guid cartId, CancellationToken ct = default);

    Task<CartItem?> FindItemAsync(Guid cartId, Guid productId, CancellationToken ct = default);

    Task AddCartItemAsync(CartItem item, CancellationToken ct = default);

    Task UpdateCartItemAsync(CartItem item, CancellationToken ct = default);

    Task RemoveItemAsync(CartItem item, CancellationToken ct = default);

    Task SetCouponAsync(Guid cartId, Guid? couponId, CancellationToken ct = default);

    Task DeleteCartAsync(Cart cart, CancellationToken ct = default);
}

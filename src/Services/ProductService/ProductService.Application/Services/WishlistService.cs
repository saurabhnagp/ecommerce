using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;

namespace AmCart.ProductService.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlist;
    private readonly IProductRepository _products;

    public WishlistService(IWishlistRepository wishlist, IProductRepository products)
    {
        _wishlist = wishlist;
        _products = products;
    }

    public async Task<IReadOnlyList<WishlistItemDto>> GetItemsAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _wishlist.GetItemsForUserAsync(userId, ct);
        return items.Select(i => i.ToDto()).ToList();
    }

    public Task<IReadOnlyList<Guid>> GetProductIdsAsync(Guid userId, CancellationToken ct = default) =>
        _wishlist.GetProductIdsForUserAsync(userId, ct);

    public async Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, publicOnly: true, ct);
        if (product == null)
            throw new InvalidOperationException("Product not found or is not available for the storefront.");

        await _wishlist.AddItemAsync(userId, productId, ct);
    }

    public Task<bool> RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        _wishlist.RemoveItemAsync(userId, productId, ct);

    public Task<bool> ContainsAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        _wishlist.ContainsAsync(userId, productId, ct);
}

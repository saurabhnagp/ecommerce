using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ProductDbContext _db;

    public CartRepository(ProductDbContext db) => _db = db;

    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default) =>
        _db.Carts.FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);

    public Task<Cart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken ct = default) =>
        _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);

    public async Task<IReadOnlyList<CartItem>> ListItemsWithProductsAsync(Guid cartId, CancellationToken ct = default)
    {
        return await _db.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cartId)
            .Include(i => i.Product!)
                .ThenInclude(p => p!.Images)
            .ToListAsync(ct);
    }

    public Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken ct = default) =>
        _db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cartId, ct);

    public async Task<Cart> GetOrCreateAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        if (userId != null)
        {
            var byUser = await GetByUserIdAsync(userId.Value, ct);
            if (byUser != null) return byUser;
            var userCart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(userCart);
            await _db.SaveChangesAsync(ct);
            return userCart;
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var bySession = await GetBySessionIdAsync(sessionId, ct);
            if (bySession != null) return bySession;
            var guestCart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = null,
                SessionId = sessionId.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(guestCart);
            await _db.SaveChangesAsync(ct);
            return guestCart;
        }

        throw new ArgumentException("Either userId or sessionId is required.");
    }

    public Task<CartItem?> FindItemAsync(Guid cartId, Guid productId, CancellationToken ct = default) =>
        _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId, ct);

    public async Task AddCartItemAsync(CartItem item, CancellationToken ct = default)
    {
        _db.CartItems.Add(item);
        await TouchCartAsync(item.CartId, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateCartItemAsync(CartItem item, CancellationToken ct = default)
    {
        await TouchCartAsync(item.CartId, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveItemAsync(CartItem item, CancellationToken ct = default)
    {
        var cartId = item.CartId;
        _db.CartItems.Remove(item);
        await TouchCartAsync(cartId, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SetCouponAsync(Guid cartId, Guid? couponId, CancellationToken ct = default)
    {
        var cart = await _db.Carts.FirstAsync(c => c.Id == cartId, ct);
        cart.AppliedCouponId = couponId;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteCartAsync(Cart cart, CancellationToken ct = default)
    {
        _db.Carts.Remove(cart);
        await _db.SaveChangesAsync(ct);
    }

    private async Task TouchCartAsync(Guid cartId, CancellationToken ct)
    {
        var cart = await _db.Carts.FirstAsync(c => c.Id == cartId, ct);
        cart.UpdatedAt = DateTime.UtcNow;
    }
}

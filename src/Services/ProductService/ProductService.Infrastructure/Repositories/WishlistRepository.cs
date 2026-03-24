using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ProductDbContext _db;

    public WishlistRepository(ProductDbContext db) => _db = db;

    public async Task<IReadOnlyList<WishlistItem>> GetItemsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.WishlistItems
            .AsNoTracking()
            .Where(i => i.Wishlist!.UserId == userId)
            .Include(i => i.Wishlist)
            .Include(i => i.Product!)
                .ThenInclude(p => p!.Images)
            .OrderByDescending(i => i.AddedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetProductIdsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var wishlistId = await _db.Wishlists
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(ct);

        if (wishlistId == null)
            return Array.Empty<Guid>();

        return await _db.WishlistItems
            .AsNoTracking()
            .Where(i => i.WishlistId == wishlistId)
            .Select(i => i.ProductId)
            .ToListAsync(ct);
    }

    public async Task<bool> AddItemAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var wishlist = await _db.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId, ct);
        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Wishlists.Add(wishlist);
            await _db.SaveChangesAsync(ct);
        }

        if (await _db.WishlistItems.AnyAsync(i => i.WishlistId == wishlist.Id && i.ProductId == productId, ct))
            return true;

        _db.WishlistItems.Add(new WishlistItem
        {
            Id = Guid.NewGuid(),
            WishlistId = wishlist.Id,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        });
        wishlist.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var item = await _db.WishlistItems
            .Include(i => i.Wishlist)
            .FirstOrDefaultAsync(i => i.Wishlist!.UserId == userId && i.ProductId == productId, ct);

        if (item == null)
            return false;

        var wishlistId = item.WishlistId;
        _db.WishlistItems.Remove(item);

        var wishlist = await _db.Wishlists.FirstAsync(w => w.Id == wishlistId, ct);
        wishlist.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ContainsAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var wishlistId = await _db.Wishlists
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(ct);

        if (wishlistId == null)
            return false;

        return await _db.WishlistItems
            .AsNoTracking()
            .AnyAsync(i => i.WishlistId == wishlistId && i.ProductId == productId, ct);
    }
}

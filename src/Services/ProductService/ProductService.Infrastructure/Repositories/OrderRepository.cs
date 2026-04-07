using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ProductDbContext _db;

    public OrderRepository(ProductDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> ListByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Images)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Order?> GetByIdForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
    }
}

using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly ProductDbContext _db;

    public CouponRepository(ProductDbContext db) => _db = db;

    public Task<Coupon?> GetByCodeAsync(string normalizedCode, CancellationToken ct = default) =>
        _db.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalizedCode && c.IsActive, ct);

    public Task<Coupon?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
}

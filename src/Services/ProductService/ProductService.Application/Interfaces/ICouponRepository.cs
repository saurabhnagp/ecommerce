using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string normalizedCode, CancellationToken ct = default);

    Task<Coupon?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

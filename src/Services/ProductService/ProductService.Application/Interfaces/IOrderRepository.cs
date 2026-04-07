using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface IOrderRepository
{
    Task<(IReadOnlyList<Order> Items, int TotalCount)> ListByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Order?> GetByIdForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default);
}

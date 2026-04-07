using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;

namespace AmCart.ProductService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;

    public OrderService(IOrderRepository orders) => _orders = orders;

    public async Task<PagedOrdersDto> ListMyOrdersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, total) = await _orders.ListByUserIdAsync(userId, page, pageSize, ct);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        return new PagedOrdersDto
        {
            Items = items.Select(OrderMapping.ToHistoryItemDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        };
    }

    public async Task<OrderConfirmationDto?> GetMyOrderAsync(
        Guid userId,
        Guid orderId,
        CancellationToken ct = default)
    {
        var order = await _orders.GetByIdForUserAsync(orderId, userId, ct);
        return order == null ? null : OrderMapping.ToConfirmationDto(order);
    }
}

using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IOrderService
{
    Task<PagedOrdersDto> ListMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    Task<OrderConfirmationDto?> GetMyOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
}

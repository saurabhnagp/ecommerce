using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICheckoutRepository
{
    Task<OrderConfirmationDto> PlaceOrderAsync(
        Guid cartId,
        Guid? cartUserId,
        string? cartSessionId,
        PlaceOrderRequest request,
        CancellationToken ct = default);
}

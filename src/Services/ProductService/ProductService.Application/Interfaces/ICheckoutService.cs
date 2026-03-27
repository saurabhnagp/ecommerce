using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICheckoutService
{
    Task<OrderConfirmationDto> PlaceOrderAsync(
        Guid? userId,
        string? sessionId,
        PlaceOrderRequest request,
        CancellationToken ct = default);
}

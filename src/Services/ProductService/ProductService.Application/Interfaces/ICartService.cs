using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid? userId, string? sessionId, CancellationToken ct = default);

    Task<CartDto> AddItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default);

    Task<CartDto> UpdateItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default);

    Task<CartDto> RemoveItemAsync(Guid? userId, string? sessionId, Guid productId, CancellationToken ct = default);

    Task<CartDto> ApplyCouponAsync(Guid? userId, string? sessionId, string code, CancellationToken ct = default);

    Task<CartDto> RemoveCouponAsync(Guid? userId, string? sessionId, CancellationToken ct = default);

    Task MergeGuestCartAsync(Guid userId, string sessionId, CancellationToken ct = default);
}

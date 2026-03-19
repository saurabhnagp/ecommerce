using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken ct = default);
    Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId, CancellationToken ct = default);
    Task<AddressDto> CreateAsync(Guid userId, CreateAddressRequest request, CancellationToken ct = default);
    Task<AddressDto?> UpdateAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct = default);
}

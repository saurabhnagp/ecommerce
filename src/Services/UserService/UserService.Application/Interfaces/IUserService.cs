using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto?> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAccountAsync(Guid id, CancellationToken ct = default);
}

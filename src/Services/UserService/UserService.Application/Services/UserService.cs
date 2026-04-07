using AmCart.UserService.Application.Common;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;

namespace AmCart.UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        return user?.ToDto();
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null) return null;

        if (request.FirstName != null) user.FirstName = request.FirstName.Trim();
        if (request.LastName != null) user.LastName = request.LastName.Trim();
        if (request.Phone != null) user.Phone = request.Phone.Trim();
        if (request.Gender != null) user.Gender = request.Gender;
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        return user.ToDto();
    }

    public async Task<bool> DeactivateAccountAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null) return false;

        user.Status = "deactivated";
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        return true;
    }
}

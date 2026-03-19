using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Common;

public static class UserMapping
{
    public static UserDto ToDto(this User user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrEmpty(name)) name = user.Email;
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = name,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Role = user.Role,
            Status = user.Status,
            IsVerified = user.IsEmailVerified,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}

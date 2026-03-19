using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Url, MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

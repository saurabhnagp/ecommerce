using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = null!;

    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = null!;

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }
}

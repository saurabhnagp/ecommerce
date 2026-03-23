using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string ConfirmPassword { get; set; } = null!;

    /// <summary>Kept for backward compatibility — ignored if FirstName/LastName are provided.</summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    [Required, Phone, MaxLength(20)]
    public string Phone { get; set; } = null!;

    [MaxLength(20)]
    public string? Gender { get; set; }
}

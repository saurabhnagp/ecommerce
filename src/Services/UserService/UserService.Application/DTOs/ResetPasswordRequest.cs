using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = null!;

    [Required]
    public string ConfirmPassword { get; set; } = null!;
}

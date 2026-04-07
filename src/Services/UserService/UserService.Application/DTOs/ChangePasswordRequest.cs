using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; set; } = null!;

    [Required]
    public string ConfirmNewPassword { get; set; } = null!;
}

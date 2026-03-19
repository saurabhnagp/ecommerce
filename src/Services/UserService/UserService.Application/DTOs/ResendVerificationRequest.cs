using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class ResendVerificationRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}

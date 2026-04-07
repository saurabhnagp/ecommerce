using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}

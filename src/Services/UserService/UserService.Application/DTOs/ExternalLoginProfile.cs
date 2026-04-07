namespace AmCart.UserService.Application.DTOs;

public class ExternalLoginProfile
{
    public string ProviderUserId { get; set; } = null!;
    public string? Email { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? FullName { get; set; }
    public string? PictureUrl { get; set; }
}

namespace AmCart.UserService.Application.DTOs;

public class ExternalLoginRequest
{
    public string Code { get; set; } = null!;
    /// <summary>Required for Google (PKCE) and Twitter (PKCE). Optional for Facebook.</summary>
    public string? CodeVerifier { get; set; }
    /// <summary>Must match one entry in the server allowlist and the value sent to the provider.</summary>
    public string RedirectUri { get; set; } = null!;
}

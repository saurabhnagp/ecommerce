namespace AmCart.UserService.Application.Configuration;

public class OAuthProvidersOptions
{
    public const string SectionName = "OAuth";

    public OAuthProviderCredentials Google { get; set; } = new();
    public OAuthProviderCredentials Facebook { get; set; } = new();
    public OAuthProviderCredentials Twitter { get; set; } = new();
}

public class OAuthProviderCredentials
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string[] RedirectUriAllowlist { get; set; } = Array.Empty<string>();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}

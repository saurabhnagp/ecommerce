using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmCart.UserService.Application;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AmCart.UserService.Infrastructure.Services;

public class ExternalOAuthExchangeService : IExternalOAuthExchangeService
{
    private const string GoogleTokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string FacebookGraphVersion = "v21.0";
    private const string TwitterTokenEndpoint = "https://api.twitter.com/2/oauth2/token";
    private const string TwitterUserMeEndpoint = "https://api.twitter.com/2/users/me";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConfigurationManager<OpenIdConnectConfiguration> GoogleOidcConfig = new(
        "https://accounts.google.com/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever(),
        new HttpDocumentRetriever { RequireHttps = true });

    private readonly HttpClient _http;
    private readonly IOptions<OAuthProvidersOptions> _options;
    private readonly ILogger<ExternalOAuthExchangeService> _logger;

    public ExternalOAuthExchangeService(
        HttpClient http,
        IOptions<OAuthProvidersOptions> options,
        ILogger<ExternalOAuthExchangeService> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<ExternalLoginProfile?> ExchangeAsync(
        ExternalAuthProvider provider,
        string code,
        string? codeVerifier,
        string redirectUri,
        CancellationToken ct = default)
    {
        try
        {
            return provider switch
            {
                ExternalAuthProvider.Google => await ExchangeGoogleAsync(code, codeVerifier, redirectUri, ct),
                ExternalAuthProvider.Facebook => await ExchangeFacebookAsync(code, redirectUri, ct),
                ExternalAuthProvider.Twitter => await ExchangeTwitterAsync(code, codeVerifier, redirectUri, ct),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External OAuth exchange failed for {Provider}", provider);
            return null;
        }
    }

    private async Task<ExternalLoginProfile?> ExchangeGoogleAsync(
        string code,
        string? codeVerifier,
        string redirectUri,
        CancellationToken ct)
    {
        var c = _options.Value.Google;
        if (!c.IsConfigured) return null;
        if (string.IsNullOrWhiteSpace(codeVerifier))
            return null;

        var body = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = c.ClientId,
            ["client_secret"] = c.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };

        using var resp = await _http.PostAsync(
            GoogleTokenEndpoint,
            new FormUrlEncodedContent(body),
            ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Google token error: {Body}", json);
            return null;
        }

        var tokenPayload = JsonSerializer.Deserialize<GoogleTokenJson>(json, JsonOpts);
        if (string.IsNullOrWhiteSpace(tokenPayload?.IdToken))
            return null;

        var oidc = await GoogleOidcConfig.GetConfigurationAsync(ct);
        var handler = new JwtSecurityTokenHandler();
        var parms = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidc.SigningKeys,
            ValidateIssuer = true,
            ValidIssuer = oidc.Issuer,
            ValidateAudience = true,
            ValidAudience = c.ClientId,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(3)
        };

        ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(tokenPayload.IdToken, parms, out _);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google id_token validation failed");
            return null;
        }

        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub)) return null;

        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? principal.FindFirst(ClaimTypes.Email)?.Value;
        var given = principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
                    ?? principal.FindFirst("given_name")?.Value;
        var family = principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value
                     ?? principal.FindFirst("family_name")?.Value;
        var name = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                   ?? principal.FindFirst("name")?.Value;
        var picture = principal.FindFirst("picture")?.Value;

        return new ExternalLoginProfile
        {
            ProviderUserId = sub,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            GivenName = given,
            FamilyName = family,
            FullName = name,
            PictureUrl = picture
        };
    }

    private async Task<ExternalLoginProfile?> ExchangeFacebookAsync(string code, string redirectUri, CancellationToken ct)
    {
        var c = _options.Value.Facebook;
        if (!c.IsConfigured) return null;

        var url =
            $"https://graph.facebook.com/{FacebookGraphVersion}/oauth/access_token?" +
            $"client_id={Uri.EscapeDataString(c.ClientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"client_secret={Uri.EscapeDataString(c.ClientSecret)}&" +
            $"code={Uri.EscapeDataString(code)}";

        using var resp = await _http.GetAsync(url, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Facebook token error: {Body}", json);
            return null;
        }

        var tokenPayload = JsonSerializer.Deserialize<FacebookTokenJson>(json, JsonOpts);
        if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
            return null;

        var meUrl =
            $"https://graph.facebook.com/{FacebookGraphVersion}/me?" +
            "fields=id,name,email,first_name,last_name,picture.type(large)&" +
            $"access_token={Uri.EscapeDataString(tokenPayload.AccessToken)}";

        using var meResp = await _http.GetAsync(meUrl, ct);
        var meJson = await meResp.Content.ReadAsStringAsync(ct);
        if (!meResp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Facebook /me error: {Body}", meJson);
            return null;
        }

        var me = JsonSerializer.Deserialize<FacebookMeJson>(meJson, JsonOpts);
        if (me == null || string.IsNullOrWhiteSpace(me.Id))
            return null;

        var pictureUrl = me.Picture?.Data?.Url;

        return new ExternalLoginProfile
        {
            ProviderUserId = me.Id,
            Email = string.IsNullOrWhiteSpace(me.Email) ? null : me.Email.Trim().ToLowerInvariant(),
            GivenName = me.FirstName,
            FamilyName = me.LastName,
            FullName = me.Name,
            PictureUrl = pictureUrl
        };
    }

    private async Task<ExternalLoginProfile?> ExchangeTwitterAsync(
        string code,
        string? codeVerifier,
        string redirectUri,
        CancellationToken ct)
    {
        var c = _options.Value.Twitter;
        if (!c.IsConfigured) return null;
        if (string.IsNullOrWhiteSpace(codeVerifier))
            return null;

        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{c.ClientId}:{c.ClientSecret}"));
        using var req = new HttpRequestMessage(HttpMethod.Post, TwitterTokenEndpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["code_verifier"] = codeVerifier,
            ["client_id"] = c.ClientId
        });

        using var resp = await _http.SendAsync(req, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Twitter token error: {Body}", json);
            return null;
        }

        var tokenPayload = JsonSerializer.Deserialize<TwitterTokenJson>(json, JsonOpts);
        if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
            return null;

        using var meReq = new HttpRequestMessage(
            HttpMethod.Get,
            TwitterUserMeEndpoint + "?user.fields=profile_image_url,confirmed_email,name,username");
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenPayload.AccessToken);

        using var meResp = await _http.SendAsync(meReq, ct);
        var meJson = await meResp.Content.ReadAsStringAsync(ct);
        if (!meResp.IsSuccessStatusCode)
        {
            _logger.LogDebug("Twitter /users/me error: {Body}", meJson);
            return null;
        }

        using var doc = JsonDocument.Parse(meJson);
        if (!doc.RootElement.TryGetProperty("data", out var data))
            return null;

        var id = data.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var name = data.TryGetProperty("name", out var n) ? n.GetString() : null;
        var username = data.TryGetProperty("username", out var u) ? u.GetString() : null;
        var picture = data.TryGetProperty("profile_image_url", out var p) ? p.GetString() : null;
        var confirmedEmail = data.TryGetProperty("confirmed_email", out var e) ? e.GetString() : null;

        return new ExternalLoginProfile
        {
            ProviderUserId = id,
            Email = string.IsNullOrWhiteSpace(confirmedEmail)
                ? null
                : confirmedEmail.Trim().ToLowerInvariant(),
            GivenName = null,
            FamilyName = null,
            FullName = !string.IsNullOrWhiteSpace(name) ? name : username,
            PictureUrl = picture
        };
    }

    private sealed class GoogleTokenJson
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }

    private sealed class FacebookTokenJson
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class FacebookMeJson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
        public FacebookPictureJson? Picture { get; set; }
    }

    private sealed class FacebookPictureJson
    {
        public FacebookPictureDataJson? Data { get; set; }
    }

    private sealed class FacebookPictureDataJson
    {
        public string? Url { get; set; }
    }

    private sealed class TwitterTokenJson
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}

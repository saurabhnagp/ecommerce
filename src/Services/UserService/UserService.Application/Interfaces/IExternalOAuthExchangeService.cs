using AmCart.UserService.Application;
using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface IExternalOAuthExchangeService
{
    Task<ExternalLoginProfile?> ExchangeAsync(
        ExternalAuthProvider provider,
        string code,
        string? codeVerifier,
        string redirectUri,
        CancellationToken ct = default);
}

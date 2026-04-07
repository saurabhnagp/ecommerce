using Microsoft.AspNetCore.Http;

namespace AmCart.ProductService.Api.Extensions;

public static class CartHttpExtensions
{
    /// <summary>Resolves cart owner: authenticated user id, or guest <c>X-Cart-Session-Id</c> header (8–80 chars).</summary>
    public static bool TryResolveCartIdentity(this HttpContext http, out Guid? userId, out string? sessionId)
    {
        userId = null;
        sessionId = null;
        if (http.User.Identity?.IsAuthenticated == true)
        {
            userId = http.User.GetUserId();
            if (userId != null)
                return true;
        }

        if (http.Request.Headers.TryGetValue("X-Cart-Session-Id", out var header))
        {
            var s = header.ToString().Trim();
            if (s.Length is >= 8 and <= 80)
            {
                sessionId = s;
                return true;
            }
        }

        return false;
    }
}

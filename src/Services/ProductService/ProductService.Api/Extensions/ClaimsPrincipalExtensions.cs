using System.Security.Claims;

namespace AmCart.ProductService.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

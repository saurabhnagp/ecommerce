using AmCart.ProductService.Api.Extensions;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkout;

    public CheckoutController(ICheckoutService checkout)
    {
        _checkout = checkout;
    }

    /// <summary>Place order from the current cart (authenticated user or guest session header).</summary>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest? body, CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "CART_IDENTITY",
                    message = "Sign in or send header X-Cart-Session-Id (guest cart)."
                }
            });

        if (body == null)
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = "Request body is required." } });

        try
        {
            var data = await _checkout.PlaceOrderAsync(userId, sessionId, body, ct);
            return Ok(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "CHECKOUT", message = ex.Message } });
        }
    }
}

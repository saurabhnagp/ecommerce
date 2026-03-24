using AmCart.ProductService.Api.Extensions;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;

    public CartController(ICartService cart)
    {
        _cart = cart;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        var dto = await _cart.GetCartAsync(userId, sessionId, ct);
        return Ok(new { success = true, data = dto });
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddCartItemRequest? body, CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        if (body == null || body.ProductId == Guid.Empty)
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = "ProductId is required." } });

        try
        {
            var dto = await _cart.AddItemAsync(userId, sessionId, body.ProductId, body.Quantity < 1 ? 1 : body.Quantity, ct);
            return Ok(new { success = true, data = dto });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuantity(Guid productId, [FromBody] UpdateCartItemRequest? body, CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        if (body == null || body.Quantity < 1)
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = "Quantity must be at least 1." } });

        try
        {
            var dto = await _cart.UpdateItemAsync(userId, sessionId, productId, body.Quantity, ct);
            return Ok(new { success = true, data = dto });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        var dto = await _cart.RemoveItemAsync(userId, sessionId, productId, ct);
        return Ok(new { success = true, data = dto });
    }

    [HttpPost("coupon")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest? body, CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        try
        {
            var dto = await _cart.ApplyCouponAsync(userId, sessionId, body?.Code ?? string.Empty, ct);
            return Ok(new { success = true, data = dto });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [HttpDelete("coupon")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
    {
        if (!HttpContext.TryResolveCartIdentity(out var userId, out var sessionId))
            return BadRequest(new { success = false, error = new { code = "CART_IDENTITY", message = "Sign in or send header X-Cart-Session-Id (guest cart)." } });

        var dto = await _cart.RemoveCouponAsync(userId, sessionId, ct);
        return Ok(new { success = true, data = dto });
    }

    [Authorize]
    [HttpPost("merge")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Merge([FromBody] MergeCartRequest? body, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        if (body == null || string.IsNullOrWhiteSpace(body.SessionId))
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = "SessionId is required." } });

        await _cart.MergeGuestCartAsync(userId.Value, body.SessionId.Trim(), ct);
        var dto = await _cart.GetCartAsync(userId, null, ct);
        return Ok(new { success = true, data = dto });
    }
}

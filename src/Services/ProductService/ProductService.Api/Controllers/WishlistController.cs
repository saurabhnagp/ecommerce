using AmCart.ProductService.Api.Extensions;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlist;

    public WishlistController(IWishlistService wishlist)
    {
        _wishlist = wishlist;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var items = await _wishlist.GetItemsAsync(userId.Value, ct);
        return Ok(new { success = true, data = items });
    }

    [HttpGet("product-ids")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductIds(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var ids = await _wishlist.GetProductIdsAsync(userId.Value, ct);
        return Ok(new { success = true, data = ids });
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddWishlistItemRequest? body, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        if (body == null || body.ProductId == Guid.Empty)
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = "ProductId is required." } });

        try
        {
            await _wishlist.AddAsync(userId.Value, body.ProductId, ct);
            return Ok(new { success = true, message = "Added to wishlist." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var removed = await _wishlist.RemoveAsync(userId.Value, productId, ct);
        if (!removed)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Item not in wishlist." } });

        return Ok(new { success = true, message = "Removed from wishlist." });
    }

    [HttpGet("items/{productId:guid}/exists")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Exists(Guid productId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var exists = await _wishlist.ContainsAsync(userId.Value, productId, ct);
        return Ok(new { success = true, data = new { exists } });
    }
}

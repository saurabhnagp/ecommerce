using AmCart.ProductService.Api.Extensions;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Sign in to view orders." } });

        var data = await _orders.ListMyOrdersAsync(userId.Value, page, pageSize, ct);
        return Ok(new { success = true, data });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Sign in to view orders." } });

        var data = await _orders.GetMyOrderAsync(userId.Value, id, ct);
        if (data == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Order not found." } });

        return Ok(new { success = true, data });
    }
}

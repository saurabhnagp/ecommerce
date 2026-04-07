using AmCart.UserService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

/// <summary>
/// BFF: forwards order APIs to ProductService so clients can use the user service base URL.
/// Returns 503 with "Order service is temporarily unavailable" when the outbound HTTP call to ProductService fails
/// (<see cref="HttpRequestException"/>), e.g. ProductService is down, wrong <c>ProductService:BaseUrl</c>, or network/DNS issues.
/// The SPA can call ProductService <c>/api/v1/orders</c> directly (same JWT) to avoid this hop in local dev.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/users/me")]
public class MeOrdersController : ControllerBase
{
    private readonly ProductServiceProxy _productOrders;

    public MeOrdersController(ProductServiceProxy productOrders)
    {
        _productOrders = productOrders;
    }

    [HttpGet("orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ListOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Missing bearer token." } });

        try
        {
            var resp = await _productOrders.GetOrdersAsync(page, pageSize, auth, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)resp.StatusCode,
                Content = body,
                ContentType = "application/json",
            };
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    success = false,
                    error = new
                    {
                        code = "PRODUCT_SERVICE_UNAVAILABLE",
                        message = "Order service is temporarily unavailable. Try again later.",
                    },
                });
        }
    }

    [HttpGet("orders/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct = default)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Missing bearer token." } });

        try
        {
            var resp = await _productOrders.GetOrderAsync(orderId, auth, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)resp.StatusCode,
                Content = body,
                ContentType = "application/json",
            };
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    success = false,
                    error = new
                    {
                        code = "PRODUCT_SERVICE_UNAVAILABLE",
                        message = "Order service is temporarily unavailable. Try again later.",
                    },
                });
        }
    }
}

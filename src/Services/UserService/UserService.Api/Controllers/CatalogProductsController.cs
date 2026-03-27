using AmCart.UserService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

/// <summary>
/// BFF: public product detail and neighbors via UserService base URL (optional; SPA may call ProductService directly).
/// </summary>
[ApiController]
[Route("api/v1/catalog/products")]
public class CatalogProductsController : ControllerBase
{
    private readonly ProductServiceProxy _productService;

    public CatalogProductsController(ProductServiceProxy productService)
    {
        _productService = productService;
    }

    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var auth = Request.Headers.Authorization.ToString();
        try
        {
            var resp = await _productService.GetProductBySlugAsync(slug, auth, ct);
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
                        message = "Catalog service is temporarily unavailable. Try again later.",
                    },
                });
        }
    }

    [HttpGet("{productId:guid}/neighbors")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetNeighbors(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _productService.GetProductNeighborsAsync(productId, ct);
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
                        message = "Catalog service is temporarily unavailable. Try again later.",
                    },
                });
        }
    }
}

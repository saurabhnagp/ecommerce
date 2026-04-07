using System.Text.Json;
using AmCart.UserService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

/// <summary>BFF: authenticated product review create/vote via UserService (optional; SPA may call ProductService directly).</summary>
[ApiController]
[Authorize]
[Route("api/v1/users/me")]
public class MeProductReviewsController : ControllerBase
{
    private readonly ProductServiceProxy _productService;

    public MeProductReviewsController(ProductServiceProxy productService)
    {
        _productService = productService;
    }

    [HttpPost("products/{productId:guid}/reviews")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateReview(Guid productId, [FromBody] JsonElement body, CancellationToken ct = default)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Missing bearer token." } });

        try
        {
            var json = body.GetRawText();
            var resp = await _productService.PostProductReviewAsync(productId, json, auth, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)resp.StatusCode,
                Content = responseBody,
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

    [HttpPost("products/{productId:guid}/reviews/{reviewId:guid}/vote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> VoteReview(
        Guid productId,
        Guid reviewId,
        [FromBody] JsonElement body,
        CancellationToken ct = default)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Missing bearer token." } });

        try
        {
            var json = body.GetRawText();
            var resp = await _productService.PostProductReviewVoteAsync(productId, reviewId, json, auth, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)resp.StatusCode,
                Content = responseBody,
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

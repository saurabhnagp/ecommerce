using System.Security.Claims;
using AmCart.ProductService.Api.Extensions;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productId:guid}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewService _reviewService;

    public ProductReviewsController(IProductReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentUserId = User.GetUserId();
        var result = await _reviewService.GetByProductIdAsync(productId, page, pageSize, currentUserId, ct);
        return Ok(new { success = true, data = result });
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductReviewRequest request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var displayName = User.FindFirstValue(ClaimTypes.Name)?.Trim();
        try
        {
            var review = await _reviewService.CreateAsync(productId, userId.Value, displayName, request, ct);
            return CreatedAtAction(nameof(GetById), new { productId, id = review.Id }, new { success = true, data = review });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [Authorize]
    [HttpPost("{reviewId:guid}/vote")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vote(
        Guid productId,
        Guid reviewId,
        [FromBody] ReviewVoteRequest request,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        try
        {
            var review = await _reviewService.VoteAsync(productId, reviewId, userId.Value, request, ct);
            if (review == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
            return Ok(new { success = true, data = review });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid productId, Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetByIdAsync(id, ct);
        if (review == null || review.ProductId != productId)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
        return Ok(new { success = true, data = review });
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid productId, Guid id, [FromBody] UpdateProductReviewRequest? request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Invalid user context." } });

        var isAdmin = User.IsInRole("admin");
        try
        {
            var review = await _reviewService.UpdateAsync(
                id,
                userId.Value,
                isAdmin,
                request?.Rating,
                request?.Title,
                request?.Comment,
                ct);
            if (review == null || review.ProductId != productId)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
            return Ok(new { success = true, data = review });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid productId, Guid id, CancellationToken ct = default)
    {
        var approved = await _reviewService.ApproveAsync(id, ct);
        if (!approved)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
        return Ok(new { success = true, message = "Review approved." });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid productId, Guid id, CancellationToken ct = default)
    {
        var deleted = await _reviewService.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
        return Ok(new { success = true, message = "Review deleted." });
    }
}

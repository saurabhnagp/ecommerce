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
    public async Task<IActionResult> GetPaged(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _reviewService.GetByProductIdAsync(productId, page, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductReviewRequest request, CancellationToken ct = default)
    {
        try
        {
            var review = await _reviewService.CreateAsync(productId, request, ct);
            return CreatedAtAction(nameof(GetById), new { productId, id = review.Id }, new { success = true, data = review });
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid productId, Guid id, [FromBody] UpdateProductReviewRequest? request, CancellationToken ct = default)
    {
        var review = await _reviewService.UpdateAsync(id, request?.Rating, request?.Title, request?.Comment, ct);
        if (review == null || review.ProductId != productId)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Review not found." } });
        return Ok(new { success = true, data = review });
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

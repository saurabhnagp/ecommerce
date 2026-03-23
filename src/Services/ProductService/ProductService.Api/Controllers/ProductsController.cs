using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] bool publicOnly = true,
        CancellationToken ct = default)
    {
        var result = await _productService.GetPagedAsync(page, pageSize, categoryId, brandId, status, search, minPrice, maxPrice, sortBy, sortDesc, defaultToActiveStatus: publicOnly, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("featured")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var items = await _productService.GetFeaturedAsync(count, ct);
        return Ok(new { success = true, data = items });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool publicOnly = true, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, publicOnly, ct);
        if (product == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Product not found." } });
        return Ok(new { success = true, data = product });
    }

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] bool publicOnly = true, CancellationToken ct = default)
    {
        var product = await _productService.GetBySlugAsync(slug, publicOnly, ct);
        if (product == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Product not found." } });
        return Ok(new { success = true, data = product });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var product = await _productService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { success = true, data = product });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, request, ct);
            if (product == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Product not found." } });
            return Ok(new { success = true, data = product });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.PublishAsync(id, ct);
        if (product == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Product not found." } });
        return Ok(new { success = true, data = product, message = "Product published." });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await _productService.SoftDeleteAsync(id, ct);
        if (!deleted)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Product not found." } });
        return Ok(new { success = true, message = "Product deleted." });
    }
}

using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var list = await _brandService.GetAllAsync(includeInactive, ct);
        return Ok(new { success = true, data = list });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var brand = await _brandService.GetByIdAsync(id, ct);
        if (brand == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Brand not found." } });
        return Ok(new { success = true, data = brand });
    }

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var brand = await _brandService.GetBySlugAsync(slug, ct);
        if (brand == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Brand not found." } });
        return Ok(new { success = true, data = brand });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request, CancellationToken ct = default)
    {
        try
        {
            var brand = await _brandService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = brand.Id }, new { success = true, data = brand });
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken ct = default)
    {
        try
        {
            var brand = await _brandService.UpdateAsync(id, request, ct);
            if (brand == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Brand not found." } });
            return Ok(new { success = true, data = brand });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            var deleted = await _brandService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Brand not found." } });
            return Ok(new { success = true, message = "Brand deleted." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }
}

using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var list = await _categoryService.GetAllAsync(includeInactive, ct);
        return Ok(new { success = true, data = list });
    }

    [HttpGet("roots")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoots(CancellationToken ct = default)
    {
        var list = await _categoryService.GetRootCategoriesAsync(ct);
        return Ok(new { success = true, data = list });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Category not found." } });
        return Ok(new { success = true, data = category });
    }

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var category = await _categoryService.GetBySlugAsync(slug, ct);
        if (category == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Category not found." } });
        return Ok(new { success = true, data = category });
    }

    [HttpGet("{id:guid}/subcategories")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubCategories(Guid id, CancellationToken ct = default)
    {
        var list = await _categoryService.GetSubCategoriesAsync(id, ct);
        return Ok(new { success = true, data = list });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, new { success = true, data = category });
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request, ct);
            if (category == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Category not found." } });
            return Ok(new { success = true, data = category });
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
            var deleted = await _categoryService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Category not found." } });
            return Ok(new { success = true, message = "Category deleted." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION", message = ex.Message } });
        }
    }
}

using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/testimonials")]
public class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _service;

    public TestimonialsController(ITestimonialService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var items = await _service.GetActiveAsync(ct);
        return Ok(new { success = true, data = items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item == null) return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Testimonial not found." } });
        return Ok(new { success = true, data = item });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialRequest request, CancellationToken ct)
    {
        var item = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, new { success = true, data = item });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialRequest request, CancellationToken ct)
    {
        var item = await _service.UpdateAsync(id, request, ct);
        if (item == null) return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Testimonial not found." } });
        return Ok(new { success = true, data = item });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        if (!deleted) return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Testimonial not found." } });
        return Ok(new { success = true, message = "Testimonial deleted." });
    }
}

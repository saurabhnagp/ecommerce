using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/contact")]
public class ContactController : ControllerBase
{
    private readonly IContactMessageService _service;

    public ContactController(IContactMessageService service) => _service = service;

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateContactMessageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var item = await _service.SubmitAsync(request, ct);
        return Ok(new { success = true, data = item, message = "Your message has been sent successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _service.GetAllAsync(ct);
        return Ok(new { success = true, data = items });
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var ok = await _service.MarkAsReadAsync(id, ct);
        if (!ok) return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Message not found." } });
        return Ok(new { success = true, message = "Marked as read." });
    }
}

using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _service;

    public NewsletterController(INewsletterService service) => _service = service;

    [Authorize(Roles = "admin")]
    [HttpGet("subscribers")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _service.GetAllAsync(ct);
        return Ok(new { success = true, data = items });
    }

    [AllowAnonymous]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var item = await _service.SubscribeAsync(request.Email, ct);
        return Ok(new { success = true, data = item, message = "Subscribed successfully." });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] SubscribeNewsletterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ok = await _service.UnsubscribeAsync(request.Email, ct);
        if (!ok) return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Subscription not found or already inactive." } });
        return Ok(new { success = true, message = "Unsubscribed successfully." });
    }
}

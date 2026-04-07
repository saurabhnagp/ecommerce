using Microsoft.AspNetCore.Mvc;

namespace AmCart.ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", service = "ProductService" });
}

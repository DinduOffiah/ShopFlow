using Microsoft.AspNetCore.Mvc;

namespace ShopFlow.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new
    {
        service = "ShopFlow.Identity",
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
}

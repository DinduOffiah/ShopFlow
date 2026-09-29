using Microsoft.AspNetCore.Mvc;

namespace ShopFlow.Catalog.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new
    {
        service = "ShopFlow.Catalog",
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
}

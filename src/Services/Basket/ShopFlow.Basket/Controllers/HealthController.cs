using Microsoft.AspNetCore.Mvc;

namespace ShopFlow.Basket.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new
    {
        service = "ShopFlow.Basket",
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
}

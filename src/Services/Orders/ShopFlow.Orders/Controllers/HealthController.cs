using Microsoft.AspNetCore.Mvc;

namespace ShopFlow.Orders.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new
    {
        service = "ShopFlow.Orders",
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
}

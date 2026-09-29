using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.Basket.Models;
using ShopFlow.Basket.Services;

namespace ShopFlow.Basket.Controllers;

[ApiController]
[Route("api/v1/basket")]
[Authorize]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;

    public BasketController(IBasketRepository repository) => _repository = repository;

    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var basket = await _repository.GetBasketAsync(userId)
                     ?? new CustomerBasketDto(userId, Array.Empty<BasketItemDto>(), 0);

        return Ok(new { success = true, data = basket });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddBasketItemRequest request)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (request.Quantity < 1)
            return BadRequest(new { success = false, message = "Quantity must be at least 1." });

        if (request.UnitPrice < 0)
            return BadRequest(new { success = false, message = "Unit price must be non-negative." });

        var existing = await _repository.GetBasketAsync(userId);
        var items = existing?.Items.ToList() ?? new List<BasketItemDto>();

        var index = items.FindIndex(i => i.ProductId == request.ProductId);
        if (index >= 0)
        {
            var current = items[index];
            items[index] = current with { Quantity = current.Quantity + request.Quantity };
        }
        else
        {
            items.Add(new BasketItemDto(
                request.ProductId,
                request.ProductName,
                request.UnitPrice,
                request.Quantity));
        }

        var total = items.Sum(i => i.UnitPrice * i.Quantity);
        var basket = new CustomerBasketDto(userId, items, total);
        await _repository.UpdateBasketAsync(userId, basket);

        return Ok(new { success = true, message = "Item added to basket.", data = basket });
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid productId, [FromBody] UpdateBasketItemRequest request)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var existing = await _repository.GetBasketAsync(userId);
        if (existing is null)
            return NotFound(new { success = false, message = "Basket is empty." });

        var items = existing.Items.ToList();
        var index = items.FindIndex(i => i.ProductId == productId);
        if (index < 0)
            return NotFound(new { success = false, message = "Item not in basket." });

        if (request.Quantity < 1)
        {
            items.RemoveAt(index);
        }
        else
        {
            items[index] = items[index] with { Quantity = request.Quantity };
        }

        var total = items.Sum(i => i.UnitPrice * i.Quantity);
        var basket = new CustomerBasketDto(userId, items, total);

        if (items.Count == 0)
            await _repository.DeleteBasketAsync(userId);
        else
            await _repository.UpdateBasketAsync(userId, basket);

        return Ok(new { success = true, message = "Basket updated.", data = basket });
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var existing = await _repository.GetBasketAsync(userId);
        if (existing is null)
            return NotFound(new { success = false, message = "Basket is empty." });

        var items = existing.Items.Where(i => i.ProductId != productId).ToList();
        var total = items.Sum(i => i.UnitPrice * i.Quantity);
        var basket = new CustomerBasketDto(userId, items, total);

        if (items.Count == 0)
            await _repository.DeleteBasketAsync(userId);
        else
            await _repository.UpdateBasketAsync(userId, basket);

        return Ok(new { success = true, message = "Item removed.", data = basket });
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _repository.DeleteBasketAsync(userId);
        return Ok(new { success = true, message = "Basket cleared." });
    }
}

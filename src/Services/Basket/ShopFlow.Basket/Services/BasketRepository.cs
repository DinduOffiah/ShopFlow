using System.Text.Json;
using ShopFlow.Basket.Models;
using StackExchange.Redis;

namespace ShopFlow.Basket.Services;

public interface IBasketRepository
{
    Task<CustomerBasketDto?> GetBasketAsync(string userId);
    Task<CustomerBasketDto> UpdateBasketAsync(string userId, CustomerBasketDto basket);
    Task DeleteBasketAsync(string userId);
}

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BasketRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string Key(string userId) => $"basket:{userId}";

    public async Task<CustomerBasketDto?> GetBasketAsync(string userId)
    {
        var value = await _db.StringGetAsync(Key(userId));
        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<CustomerBasketDto>(value!, JsonOptions);
    }

    public async Task<CustomerBasketDto> UpdateBasketAsync(string userId, CustomerBasketDto basket)
    {
        var payload = JsonSerializer.Serialize(basket, JsonOptions);
        await _db.StringSetAsync(Key(userId), payload, TimeSpan.FromDays(7));
        return basket;
    }

    public async Task DeleteBasketAsync(string userId)
    {
        await _db.KeyDeleteAsync(Key(userId));
    }
}

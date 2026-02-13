using System.Text.Json;
using DiscountManager.Modules.Basket.Domain;
using StackExchange.Redis;

namespace DiscountManager.Modules.Basket.Infrastructure;

public class RedisBasketRepository : IBasketRepository
{
    private readonly IDatabase _database;

    public RedisBasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<CustomerBasket?> GetBasketAsync(Guid userId)
    {
        var data = await _database.StringGetAsync($"basket:{userId}");
        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var data = JsonSerializer.Serialize(basket);
        await _database.StringSetAsync($"basket:{basket.UserId}", data);
        return basket;
    }

    public async Task DeleteBasketAsync(Guid userId)
    {
        await _database.KeyDeleteAsync($"basket:{userId}");
    }
}

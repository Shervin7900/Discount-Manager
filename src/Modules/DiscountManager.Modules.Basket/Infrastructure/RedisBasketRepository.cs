using System.Collections.Concurrent;
using System.Text.Json;
using DiscountManager.Modules.Basket.Domain;
using StackExchange.Redis;

namespace DiscountManager.Modules.Basket.Infrastructure;

public class RedisBasketRepository : IBasketRepository
{
    private readonly IDatabase? _database;
    private static readonly ConcurrentDictionary<Guid, string> _fallbackStorage = new();

    public RedisBasketRepository(IConnectionMultiplexer redis)
    {
        try
        {
            _database = redis.GetDatabase();
        }
        catch
        {
            _database = null;
        }
    }

    public async Task<CustomerBasket?> GetBasketAsync(Guid userId)
    {
        try
        {
            if (_database != null)
            {
                var data = await _database.StringGetAsync($"basket:{userId}");
                if (!data.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis error (Get): {ex.Message}. Falling back to memory.");
        }

        if (_fallbackStorage.TryGetValue(userId, out var fallbackData))
        {
            return JsonSerializer.Deserialize<CustomerBasket>(fallbackData);
        }

        return null;
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var data = JsonSerializer.Serialize(basket);

        try
        {
            if (_database != null)
            {
                await _database.StringSetAsync($"basket:{basket.UserId}", data);
                return basket;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis error (Update): {ex.Message}. Falling back to memory.");
        }

        _fallbackStorage[basket.UserId] = data;
        return basket;
    }

    public async Task DeleteBasketAsync(Guid userId)
    {
        try
        {
            if (_database != null)
            {
                await _database.KeyDeleteAsync($"basket:{userId}");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis error (Delete): {ex.Message}.");
        }

        _fallbackStorage.TryRemove(userId, out _);
    }
}

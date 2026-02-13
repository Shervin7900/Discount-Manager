using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Text.Json;

namespace DiscountManager.Modules.Search.Infrastructure;

public class RedisSearchService : ISearchService
{
    private readonly IDatabase? _db;
    private static readonly ConcurrentDictionary<Guid, SearchProductData> _fallbackProducts = new();
    private static readonly ConcurrentDictionary<string, ConcurrentBag<Guid>> _fallbackIndex = new();

    public RedisSearchService(IConnectionMultiplexer redis)
    {
        try 
        {
            _db = redis.GetDatabase();
        }
        catch 
        {
            _db = null;
        }
    }

    public async Task IndexProductAsync(Guid productId, string name, string description, decimal price, Guid shopId, bool hasDiscount)
    {
        var productData = new SearchProductData(productId, name, description, price, shopId, hasDiscount);

        try
        {
            if (_db != null)
            {
                await _db.StringSetAsync($"product:{productId}", JsonSerializer.Serialize(productData));
                var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    await _db.SetAddAsync($"index:name:{word.ToLowerInvariant()}", productId.ToString());
                }
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search Redis Index Error: {ex.Message}. Falling back to memory.");
        }

        // Fallback
        _fallbackProducts[productId] = productData;
        var fWords = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in fWords)
        {
            var clean = word.ToLowerInvariant();
            var bag = _fallbackIndex.GetOrAdd(clean, _ => new ConcurrentBag<Guid>());
            if (!bag.Contains(productId)) bag.Add(productId);
        }
    }

    public async Task<IEnumerable<Guid>> SearchProductsAsync(string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return Enumerable.Empty<Guid>();
        var cleanWord = words[0].ToLowerInvariant();

        try
        {
            if (_db != null)
            {
                var members = await _db.SetMembersAsync($"index:name:{cleanWord}");
                var foundIds = members.Select(m => Guid.Parse(m.ToString())).ToList();
                var rankedResults = new List<(Guid Id, bool HasDiscount)>();

                foreach (var id in foundIds)
                {
                    var dataJson = await _db.StringGetAsync($"product:{id}");
                    bool hasDiscount = false;
                    if (!dataJson.IsNullOrEmpty)
                    {
                        using var doc = JsonDocument.Parse(dataJson.ToString());
                        if (doc.RootElement.TryGetProperty("HasDiscount", out var prop)) hasDiscount = prop.GetBoolean();
                    }
                    rankedResults.Add((id, hasDiscount));
                }
                return rankedResults.OrderByDescending(x => x.HasDiscount).Select(x => x.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search Redis Query Error: {ex.Message}. Falling back to memory.");
        }

        // Fallback
        if (_fallbackIndex.TryGetValue(cleanWord, out var ids))
        {
            return ids.Select(id => _fallbackProducts.TryGetValue(id, out var data) ? (Id: id, HasDiscount: data.HasDiscount) : (Id: id, HasDiscount: false))
                      .OrderByDescending(x => x.HasDiscount)
                      .Select(x => x.Id);
        }

        return Enumerable.Empty<Guid>();
    }

    private record SearchProductData(Guid Id, string Name, string Description, decimal Price, Guid ShopId, bool HasDiscount);
}

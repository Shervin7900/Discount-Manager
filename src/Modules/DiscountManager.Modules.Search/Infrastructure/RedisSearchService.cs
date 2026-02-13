using StackExchange.Redis;
using System.Text.Json;

namespace DiscountManager.Modules.Search.Infrastructure;

public class RedisSearchService : ISearchService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisSearchService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task IndexProductAsync(Guid productId, string name, string description, decimal price, Guid shopId, bool hasDiscount)
    {
        var productData = new
        {
            Id = productId,
            Name = name,
            Description = description,
            Price = price,
            ShopId = shopId,
            HasDiscount = hasDiscount
        };

        // Store product data
        await _db.StringSetAsync($"product:{productId}", JsonSerializer.Serialize(productData));

        // Index by Name words
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            var cleanWord = word.ToLowerInvariant();
            await _db.SetAddAsync($"index:name:{cleanWord}", productId.ToString());
        }

        // Store Discount Status separately for sorting if needed, 
        // but for now we'll just fetch and sort in memory for simplicity 
        // or prioritize retrieving from a specific "discounted" set if the query logic gets complex.
    }

    public async Task<IEnumerable<Guid>> SearchProductsAsync(string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return Enumerable.Empty<Guid>();

        // 1. Find matching IDs
        var cleanWord = words[0].ToLowerInvariant();
        var members = await _db.SetMembersAsync($"index:name:{cleanWord}");
        
        var foundIds = new List<Guid>();
        foreach (var member in members)
        {
            if (Guid.TryParse(member.ToString(), out var guid)) foundIds.Add(guid);
        }

        // 2. Prioritize: Fetch data to check for discounts
        // In a real Redis Search module (RediSearch), we would do this via query.
        // Here we'll do a quick lookup.
        var rankedResults = new List<(Guid Id, bool HasDiscount)>();

        foreach (var id in foundIds)
        {
            var dataJson = await _db.StringGetAsync($"product:{id}");
            bool hasDiscount = false;
            if (!dataJson.IsNullOrEmpty)
            {
                // Quick parse using JsonDocument for performance or just simple check
                 using var doc = JsonDocument.Parse(dataJson.ToString());
                 if (doc.RootElement.TryGetProperty("HasDiscount", out var prop))
                 {
                     hasDiscount = prop.GetBoolean();
                 }
            }
            rankedResults.Add((id, hasDiscount));
        }

        // 3. Sort: Discounted first
        return rankedResults
            .OrderByDescending(x => x.HasDiscount)
            .Select(x => x.Id);
    }
}

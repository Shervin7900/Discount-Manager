using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DiscountManager.Modules.Search.Infrastructure;

public class CachedSearchService : ISearchService
{
    private readonly ISearchService _innerService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private const int CacheDurationMinutes = 10;

    public CachedSearchService(ISearchService innerService, IMemoryCache memoryCache, IDistributedCache distributedCache)
    {
        _innerService = innerService;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public async Task IndexProductAsync(Guid productId, string name, string description, decimal price, Guid shopId, bool hasDiscount)
    {
        await _innerService.IndexProductAsync(productId, name, description, price, shopId, hasDiscount);
    }

    public async Task<IEnumerable<Guid>> SearchProductsAsync(string query)
    {
        var cacheKey = $"search:{query.ToLowerInvariant()}";

        // 1. L1: Memory Cache
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<Guid>? runtimeCacheResult) && runtimeCacheResult != null)
        {
            return runtimeCacheResult;
        }

        // 2. L2: Distributed Cache
        try 
        {
            var distributedCacheResult = await _distributedCache.GetStringAsync(cacheKey);
            if (distributedCacheResult != null)
            {
                var result = JsonSerializer.Deserialize<IEnumerable<Guid>>(distributedCacheResult);
                if (result != null)
                {
                    // Populate L1
                    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"WARNING: L2 Cache (Redis) read failed: {ex.Message}");
        }

        // 3. Source
        var sourceResult = await _innerService.SearchProductsAsync(query);
        var sourceList = sourceResult.ToList(); // Materialize

        // 4. Set Caches
        try
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes) };
            await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(sourceList), options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: L2 Cache (Redis) write failed: {ex.Message}");
        }
        
        _memoryCache.Set(cacheKey, sourceList, TimeSpan.FromMinutes(CacheDurationMinutes));

        return sourceList;
    }
}

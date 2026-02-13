using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using DiscountManager.Modules.Search.Infrastructure;

namespace DiscountManager.Modules.Search;

public static class SearchModuleExtensions
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConn = configuration["ConnectionStrings:Redis"] ?? "localhost";
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
        {
            try { return ConnectionMultiplexer.Connect(redisConn); }
            catch { return null!; }
        });

        services.AddScoped<RedisSearchService>();
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "DiscountManager_Search_";
        });

        services.AddScoped<ISearchService>(sp =>
        {
            var inner = sp.GetRequiredService<RedisSearchService>();
            var memCache = sp.GetRequiredService<IMemoryCache>();
            var distCache = sp.GetService<IDistributedCache>();
            return distCache == null ? inner : new CachedSearchService(inner, memCache, distCache);
        });

        return services;
    }
}

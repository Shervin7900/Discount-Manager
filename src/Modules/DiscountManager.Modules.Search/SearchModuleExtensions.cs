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
        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(configuration["ConnectionStrings:Redis"] ?? "localhost");
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            
            // Register implementations
            services.AddScoped<RedisSearchService>();
            
            // Add Caching
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["ConnectionStrings:Redis"] ?? "localhost";
                options.InstanceName = "DiscountManager_Search_";
            });

            // Register Decorator
            services.AddScoped<ISearchService>(sp =>
            {
                var inner = sp.GetRequiredService<RedisSearchService>();
                var memCache = sp.GetRequiredService<IMemoryCache>();
                var distCache = sp.GetRequiredService<IDistributedCache>();
                return new CachedSearchService(inner, memCache, distCache);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Could not connect to Redis for SearchModule. {ex.Message}");
            // Fallback to NoOp service
            services.AddScoped<ISearchService, NoOpSearchService>();
        }

        return services;
    }
}

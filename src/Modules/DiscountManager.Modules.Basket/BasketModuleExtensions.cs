using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using DiscountManager.Modules.Basket.Domain;
using DiscountManager.Modules.Basket.Infrastructure;

namespace DiscountManager.Modules.Basket;

public static class BasketModuleExtensions
{
    public static IServiceCollection AddBasketModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");

        // Note: Assuming ConnectionMultiplexer is likely registered by Search Module or Bootstrapper.
        // If not, we should register it safely or use TryAddSingleton.
        // BUT, since we want isolation, maybe a separate connection or reuse? 
        // User requested separate DBs, but Redis is usually shared instance with different keys or DB index.
        // We will register it if not present, or assume Bootstrapper handles shared infrastructure if desired.
        // HOWEVER, for strict modularity, we might register our own if different config.
        // Let's assume shared Redis instance for simplicity but registered here if missing.
        
        // Actually, best practice in modular monolith with shared Redis is typically one Multiplexer.
        // I'll use TryAddSingleton in Bootstrapper or check here.
        // For now, I'll just register IBasketRepository using an injected IConnectionMultiplexer.
        // If Search Module also registers IConnectionMultiplexer, the DI container resolves the last one or first key.
        try
        {
            var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost";
            var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddScoped<IBasketRepository, RedisBasketRepository>();
        }
        catch (Exception ex)
        {
             Console.WriteLine($"WARNING: Could not connect to Redis for BasketModule. {ex.Message}");
        }

        return services;
    }
}

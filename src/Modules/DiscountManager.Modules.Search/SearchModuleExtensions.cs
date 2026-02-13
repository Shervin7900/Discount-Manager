using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
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
            services.AddScoped<ISearchService, RedisSearchService>();
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

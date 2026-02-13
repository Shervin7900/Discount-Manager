using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Ordering.Infrastructure;
using DiscountManager.Modules.Ordering.Infrastructure.Clients;

namespace DiscountManager.Modules.Ordering;

public static class OrderingModuleExtensions
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderingDb");

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpClient<IBasketClient, BasketClient>(client =>
        {
            var baseUrl = configuration["Services:BaseUrl"] ?? "https://localhost:7001/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
        {
            var baseUrl = configuration["Services:BaseUrl"] ?? "https://localhost:7001/";
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}

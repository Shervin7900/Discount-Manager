using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Inventory.Infrastructure;

namespace DiscountManager.Modules.Inventory;

public static class InventoryModuleExtensions
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDb");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}

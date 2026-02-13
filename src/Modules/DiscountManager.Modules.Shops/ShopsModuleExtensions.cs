using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Shops.Infrastructure;

namespace DiscountManager.Modules.Shops;

public static class ShopsModuleExtensions
{
    public static IServiceCollection AddShopsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ShopsDb");

        services.AddDbContext<ShopsDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}

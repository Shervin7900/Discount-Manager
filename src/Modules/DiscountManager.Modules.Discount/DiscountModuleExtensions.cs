using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Discount.Infrastructure;

namespace DiscountManager.Modules.Discount;

public static class DiscountModuleExtensions
{
    public static IServiceCollection AddDiscountModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DiscountDb");

        services.AddDbContext<DiscountDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}

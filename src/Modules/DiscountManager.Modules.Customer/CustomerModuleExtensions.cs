using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Customer.Infrastructure;

namespace DiscountManager.Modules.Customer;

public static class CustomerModuleExtensions
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CustomerDb")));

        return services;
    }
}

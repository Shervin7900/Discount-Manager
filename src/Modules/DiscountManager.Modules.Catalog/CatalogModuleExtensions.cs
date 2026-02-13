using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Catalog.Infrastructure;

namespace DiscountManager.Modules.Catalog;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}

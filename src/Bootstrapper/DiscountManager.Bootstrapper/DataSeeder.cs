using DiscountManager.Modules.Catalog.Domain;
using DiscountManager.Modules.Catalog.Infrastructure;
using DiscountManager.Modules.Discount.Domain;
using DiscountManager.Modules.Discount.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager.Bootstrapper;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        try
        {
            var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var discountContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
            var shopsContext = scope.ServiceProvider.GetRequiredService<DiscountManager.Modules.Shops.Infrastructure.ShopsDbContext>();
            var searchService = scope.ServiceProvider.GetRequiredService<DiscountManager.Modules.Search.ISearchService>();

            // 1. Seed Shops
            if (!await shopsContext.Shops.AnyAsync())
            {
                shopsContext.Shops.AddRange(
                    new DiscountManager.Modules.Shops.Domain.Shop("TechWorld", "123 Tech St", "Best electronics", null),
                    new DiscountManager.Modules.Shops.Domain.Shop("FashionHub", "456 Style Ave", "Trendy clothes", null),
                    new DiscountManager.Modules.Shops.Domain.Shop("HomeDepotClone", "789 Fix It Dr", "Home improvements", null)
                );
                await shopsContext.SaveChangesAsync();
            }
            
            var shops = await shopsContext.Shops.ToListAsync();
            var techShop = shops.First(s => s.Name == "TechWorld");
            var fashionShop = shops.First(s => s.Name == "FashionHub");

            // 2. Seed Catalog with Discounts
            if (!await catalogContext.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    // Tech Shop - 2 Discounted
                    new Product("Gaming Laptop", "High-end gaming laptop", 1500, "Electronics", techShop.Id, 1200), // Discounted
                    new Product("Wireless Mouse", "Ergonomic wireless mouse", 50, "Electronics", techShop.Id),
                    new Product("Mechanical Keyboard", "RGB Mechanical Keyboard", 100, "Electronics", techShop.Id, 80), // Discounted
                    new Product("Monitor", "4K Monitor", 300, "Electronics", techShop.Id),
                    new Product("Headset", "Surround Sound", 80, "Electronics", techShop.Id),

                    // Fashion Shop - 2 Discounted
                    new Product("Running Shoes", "Comfortable running shoes", 80, "Apparel", fashionShop.Id, 60), // Discounted
                    new Product("T-Shirt", "Cotton T-Shirt", 20, "Apparel", fashionShop.Id),
                    new Product("Jeans", "Blue Jeans", 50, "Apparel", fashionShop.Id, 40), // Discounted
                    new Product("Jacket", "Winter Jacket", 100, "Apparel", fashionShop.Id),
                    new Product("Cap", "Baseball Cap", 15, "Apparel", fashionShop.Id)
                };

                catalogContext.Products.AddRange(products);
                await catalogContext.SaveChangesAsync();

                // 3. Index Products
                try 
                {
                    foreach (var p in products)
                    {
                        await searchService.IndexProductAsync(p.Id, p.Name, p.Description, p.Price, p.ShopId, p.SalesPrice.HasValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Redis Indexing failed during seeding: {ex.Message}");
                }
            }

            // 4. Seed Discounts (Coupons)
            if (!await discountContext.Discounts.AnyAsync())
            {
                discountContext.Discounts.AddRange(
                    new DiscountCoupon("WELCOME10", 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(30)),
                    new DiscountCoupon("SUMMER20", 20, DateTime.UtcNow, DateTime.UtcNow.AddDays(60)),
                    new DiscountCoupon("BLACKFRIDAY", 50, DateTime.UtcNow, DateTime.UtcNow.AddDays(5))
                );
                await discountContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEEDING ERROR: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }
}

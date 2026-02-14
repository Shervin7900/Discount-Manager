using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using DiscountManager.Modules.Catalog.Infrastructure;
using DiscountManager.Modules.Discount.Infrastructure;
using DiscountManager.Modules.Ordering.Infrastructure;
using DiscountManager.Modules.Payment.Infrastructure;
using DiscountManager.Modules.Shops.Infrastructure;
using DiscountManager.Modules.Inventory.Infrastructure;
using DiscountManager.Modules.Identity.Infrastructure;
using DiscountManager.Modules.Customer.Infrastructure;

namespace DiscountManager.Tests.Integration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Helper to remove and add in-memory DbContext
            void ReplaceWithInMemory<TContext>(string dbName) where TContext : DbContext
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(dbName));
            }

            ReplaceWithInMemory<CatalogDbContext>("InMemoryCatalog");
            ReplaceWithInMemory<DiscountDbContext>("InMemoryDiscount");
            ReplaceWithInMemory<OrderingDbContext>("InMemoryOrdering");
            ReplaceWithInMemory<PaymentDbContext>("InMemoryPayment");
            ReplaceWithInMemory<ShopsDbContext>("InMemoryShops");
            ReplaceWithInMemory<InventoryDbContext>("InMemoryInventory");
            ReplaceWithInMemory<IdentityDbContext>("InMemoryIdentity");
            ReplaceWithInMemory<CustomerDbContext>("InMemoryCustomer");

            // Mock Redis
            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null) services.Remove(redisDescriptor);

            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            var mockDatabase = new Mock<IDatabase>();
            mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
            services.AddSingleton(mockMultiplexer.Object);

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            
            try
            {
                scopedServices.GetRequiredService<CatalogDbContext>().Database.EnsureCreated();
                // Add others if needed
            }
            catch (Exception ex)
            {
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
            }
        });
    }
}

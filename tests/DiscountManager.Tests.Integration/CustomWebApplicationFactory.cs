using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Parbad.Builder;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Storage.EntityFrameworkCore.Builder;
using DiscountManager.Modules.Catalog.Infrastructure;
using DiscountManager.Modules.Discount.Infrastructure;
using DiscountManager.Modules.Ordering.Infrastructure;
using DiscountManager.Modules.Payment;
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

                // Use a unique name per test run instance if possible, or a stable one.
                // For integration tests, a stable name is usually fine unless tests run in parallel.
                services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(dbName));
            }

            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            ReplaceWithInMemory<CatalogDbContext>($"Catalog_{uniqueId}");
            ReplaceWithInMemory<DiscountDbContext>($"Discount_{uniqueId}");
            ReplaceWithInMemory<OrderingDbContext>($"Ordering_{uniqueId}");
            ReplaceWithInMemory<PaymentDbContext>($"Payment_{uniqueId}");
            ReplaceWithInMemory<ShopsDbContext>($"Shops_{uniqueId}");
            ReplaceWithInMemory<InventoryDbContext>($"Inventory_{uniqueId}");
            ReplaceWithInMemory<IdentityDbContext>($"Identity_{uniqueId}");
            ReplaceWithInMemory<CustomerDbContext>($"Customer_{uniqueId}");

            // Mock Redis
            // ... (keep Redis mock same)
            // We remove all registrations and add a single mock that returns a mock database.
            var redisDescriptors = services.Where(d => d.ServiceType == typeof(IConnectionMultiplexer)).ToList();
            foreach (var d in redisDescriptors) services.Remove(d);

            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            var mockDatabase = new Mock<IDatabase>();
            mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
            services.AddSingleton(mockMultiplexer.Object);

            // Re-configure Parbad for In-Memory specifically for tests
            services.AddParbad()
                .ConfigureGateways(gateways => gateways.AddParbadVirtual())
                .ConfigureStorage(storage =>
                {
                    storage.UseEfCore(ef =>
                    {
                        ef.ConfigureDbContext = db => db.UseInMemoryDatabase("InMemoryParbad");
                        ef.DefaultSchema = "payment";
                    });
                })
                .ConfigureHttpContext(builder => builder.UseDefaultAspNetCore());

            // Build the provider once to initialize databases explicitly
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var providers = scope.ServiceProvider;

            try
            {
                // Ensure all DBs are created before app starts and seeder runs
                providers.GetRequiredService<CatalogDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<ShopsDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<DiscountDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<OrderingDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<PaymentDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<InventoryDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
                providers.GetRequiredService<CustomerDbContext>().Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PRE-START ERROR: {ex.Message}");
            }
        });
    }
}

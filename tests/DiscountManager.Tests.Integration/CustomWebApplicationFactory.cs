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
        });
    }
}

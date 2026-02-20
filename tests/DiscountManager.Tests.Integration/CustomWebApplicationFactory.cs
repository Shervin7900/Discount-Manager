using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        // UseSetting is extremely fast and runs early
        builder.UseSetting("ConnectionStrings:CatalogDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:ShopsDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:DiscountDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:InventoryDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:OrderingDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:PaymentDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:IdentityDb", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:CustomerDb", "DataSource=:memory:");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContextOptions to ensure no SQL Server connections remain
            var dbContextOptions = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions) || 
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            ).ToList();
            foreach (var d in dbContextOptions) services.Remove(d);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(System.Data.Common.DbConnection));
            if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

            // Create an isolated service provider specifically for EF Core InMemory databases
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Clean up Parbad existing services
            var parbadServices = services.Where(d => d.ServiceType.Namespace?.Contains("Parbad") == true).ToList();
            foreach (var d in parbadServices) services.Remove(d);

            void ReplaceWithInMemory<TContext>(string dbName) where TContext : DbContext
            {
                services.AddDbContext<TContext>(options => 
                {
                    options.UseInMemoryDatabase(dbName);
                    options.UseInternalServiceProvider(efServiceProvider);
                    options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            }

            ReplaceWithInMemory<CatalogDbContext>("CatalogTest");
            ReplaceWithInMemory<DiscountDbContext>("DiscountTest");
            ReplaceWithInMemory<OrderingDbContext>("OrderingTest");
            ReplaceWithInMemory<PaymentDbContext>("PaymentTest");
            ReplaceWithInMemory<ShopsDbContext>("ShopsTest");
            ReplaceWithInMemory<InventoryDbContext>("InventoryTest");
            ReplaceWithInMemory<IdentityDbContext>("IdentityTest");
            ReplaceWithInMemory<CustomerDbContext>("CustomerTest");

            // Mock Redis
            var redisDescriptors = services.Where(d => d.ServiceType == typeof(IConnectionMultiplexer)).ToList();
            foreach (var d in redisDescriptors) services.Remove(d);

            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            var mockDatabase = new Mock<IDatabase>();
            mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
            services.AddSingleton(mockMultiplexer.Object);

            // Re-configure Parbad for In-Memory with isolation
            services.AddParbad()
                .ConfigureGateways(gateways => gateways.AddParbadVirtual())
                .ConfigureStorage(storage =>
                {
                    storage.UseEfCore(ef =>
                    {
                        ef.ConfigureDbContext = db => {
                            db.UseInMemoryDatabase("ParbadTest");
                            db.UseInternalServiceProvider(efServiceProvider);
                        };
                        ef.DefaultSchema = "payment";
                    });
                })
                .ConfigureHttpContext(b => b.UseDefaultAspNetCore());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Ensure databases are created before tests run
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        
        sp.GetRequiredService<CatalogDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<ShopsDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<DiscountDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<OrderingDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<PaymentDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<InventoryDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<CustomerDbContext>().Database.EnsureCreated();

        return host;
    }
}

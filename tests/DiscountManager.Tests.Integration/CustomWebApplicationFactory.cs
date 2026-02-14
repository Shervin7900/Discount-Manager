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
            // Aggressively remove EVERY service from the SQL Server provider assembly
            // This is the only way to avoid "multiple providers" error in EF Core
            var sqlServerAssemblies = new[] 
            { 
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Microsoft.Data.SqlClient"
            };

            var toRemove = services.Where(d => 
                (d.ServiceType.Assembly.FullName != null && sqlServerAssemblies.Any(a => d.ServiceType.Assembly.FullName.Contains(a))) ||
                (d.ImplementationType?.Assembly.FullName != null && sqlServerAssemblies.Any(a => d.ImplementationType.Assembly.FullName.Contains(a)))).ToList();
            
            foreach (var d in toRemove) services.Remove(d);

            // Clean up Parbad as well
            var parbadServices = services.Where(d => d.ServiceType.Namespace?.Contains("Parbad") == true).ToList();
            foreach (var d in parbadServices) services.Remove(d);

            void ReplaceWithInMemory<TContext>(string dbName) where TContext : DbContext
            {
                var removeContext = services.Where(d => 
                    d.ServiceType == typeof(TContext) || 
                    d.ServiceType == typeof(DbContextOptions<TContext>) ||
                    d.ServiceType == typeof(DbContextOptions)).ToList();
                
                foreach (var d in removeContext) services.Remove(d);

                services.AddDbContext<TContext>(options => 
                {
                    options.UseInMemoryDatabase(dbName);
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

            // Re-configure Parbad for In-Memory
            services.AddParbad()
                .ConfigureGateways(gateways => gateways.AddParbadVirtual())
                .ConfigureStorage(storage =>
                {
                    storage.UseEfCore(ef =>
                    {
                        ef.ConfigureDbContext = db => db.UseInMemoryDatabase("ParbadTest");
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

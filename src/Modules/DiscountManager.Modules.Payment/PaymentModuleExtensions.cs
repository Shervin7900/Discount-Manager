using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Builder;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Storage.EntityFrameworkCore.Builder;
using DiscountManager.Modules.Payment.Infrastructure;

namespace DiscountManager.Modules.Payment;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payment");
        base.OnModelCreating(modelBuilder);
        // Parbad configuration is automatic usually if we use the builder correctly
    }
}

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddParbad()
            .ConfigureGateways(gateways =>
            {
                gateways.AddParbadVirtual()
                    .WithOptions(o => o.GatewayPath = "/parbadvirtual"); 
            })
            .ConfigureStorage(storage =>
            {
                storage.UseEfCore(ef =>
                {
                    ef.ConfigureDbContext = db => db.UseSqlServer(connectionString);
                    ef.DefaultSchema = "payment";
                });
            })
            .ConfigureHttpContext(builder => builder.UseDefaultAspNetCore());

        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}

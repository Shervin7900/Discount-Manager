using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Shops.Domain;

namespace DiscountManager.Modules.Shops.Infrastructure;

public class ShopsDbContext : DbContext
{
    public ShopsDbContext(DbContextOptions<ShopsDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shops");
        
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
        });
    }
}

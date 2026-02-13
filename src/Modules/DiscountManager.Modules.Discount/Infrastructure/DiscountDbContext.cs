using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Discount.Domain;

namespace DiscountManager.Modules.Discount.Infrastructure;

public class DiscountDbContext : DbContext
{
    public DiscountDbContext(DbContextOptions<DiscountDbContext> options) : base(options)
    {
    }

    public DbSet<DiscountCoupon> Discounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("discount");
        
        modelBuilder.Entity<DiscountCoupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }
}

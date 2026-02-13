using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Catalog.Domain;

public class Product : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = default!;
    public Guid ShopId { get; private set; } // Reference to a Shop
    public decimal? SalesPrice { get; private set; } // Discounted Price

    public Product(string name, string description, decimal price, string category, Guid shopId, decimal? salesPrice = null)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        ShopId = shopId;
        SalesPrice = salesPrice;
    }

    // EF Core constructor
    private Product() { }

    public void UpdatePrice(decimal newPrice, decimal? salesPrice = null)
    {
        if (newPrice < 0) throw new ArgumentException("Price cannot be negative");
        Price = newPrice;
        SalesPrice = salesPrice;
    }
}

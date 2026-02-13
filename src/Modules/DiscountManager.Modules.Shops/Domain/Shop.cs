using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Shops.Domain;

public class Shop : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string? ImageUrl { get; private set; }

    public Shop(string name, string address, string description, string? imageUrl)
    {
        Name = name;
        Address = address;
        Description = description;
        ImageUrl = imageUrl;
    }

    private Shop() { } // EF Core
}

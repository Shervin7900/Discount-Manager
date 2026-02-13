using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Discount.Domain;

public class DiscountCoupon : Entity, IAggregateRoot
{
    public string Code { get; private set; } = default!;
    public decimal Percentage { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public Guid? ProductId { get; private set; } // specific to product
    public Guid? ShopId { get; private set; }    // specific to shop

    public DiscountCoupon(string code, decimal percentage, DateTime validFrom, DateTime validTo)
    {
        Code = code;
        Percentage = percentage;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
    
    private DiscountCoupon() { } // EF Core
}

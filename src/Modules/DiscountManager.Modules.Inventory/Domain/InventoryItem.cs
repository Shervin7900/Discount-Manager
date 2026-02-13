using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Inventory.Domain;

public class InventoryItem : Entity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    public InventoryItem(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public void AdjustQuantity(int adjustment)
    {
        if (Quantity + adjustment < 0) throw new InvalidOperationException("Not enough inventory");
        Quantity += adjustment;
    }

    private InventoryItem() { }
}

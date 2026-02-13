using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Ordering.Domain;

public class Order : Entity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = default!;
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(Guid userId, decimal totalAmount)
    {
        UserId = userId;
        OrderDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = "Pending";
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
    }
    
    private Order() { }
}

public class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
    
    private OrderItem() { }
}

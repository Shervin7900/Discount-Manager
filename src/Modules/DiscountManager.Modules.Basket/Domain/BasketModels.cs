namespace DiscountManager.Modules.Basket.Domain;

public class CustomerBasket
{
    public Guid UserId { get; set; }
    public List<BasketItem> Items { get; set; } = new();

    public CustomerBasket() { }
    public CustomerBasket(Guid userId)
    {
        UserId = userId;
    }
}

public class BasketItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(Guid userId);
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);
    Task DeleteBasketAsync(Guid userId);
}

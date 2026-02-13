using Microsoft.AspNetCore.Mvc;
using DiscountManager.Modules.Ordering.Domain;
using DiscountManager.Modules.Ordering.Infrastructure;
using DiscountManager.Modules.Ordering.Infrastructure.Clients;
using DiscountManager.Modules.Payment.Contracts;

namespace DiscountManager.Modules.Ordering.Infrastructure;

[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly OrderingDbContext _dbContext;
    private readonly IBasketClient _basketClient;
    private readonly IPaymentClient _paymentClient;

    public CheckoutController(OrderingDbContext dbContext, IBasketClient basketClient, IPaymentClient paymentClient)
    {
        _dbContext = dbContext;
        _basketClient = basketClient;
        _paymentClient = paymentClient;
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> Checkout(Guid userId, [FromQuery] string callbackUrl)
    {
        // 1. Get Basket
        var basket = await _basketClient.GetBasketAsync(userId);
        if (basket == null || !basket.Items.Any())
        {
            return BadRequest("Basket is empty or not found.");
        }

        // 2. Calculate Total
        decimal totalAmount = basket.Items.Sum(x => x.UnitPrice * x.Quantity);

        // 3. Create Order
        var order = new Order(userId, totalAmount);
        foreach (var item in basket.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // 4. Request Payment
        var paymentRequest = new PaymentRequestDto(order.Id, totalAmount, callbackUrl);
        var paymentResult = await _paymentClient.RequestPaymentAsync(paymentRequest);

        if (paymentResult == null || !paymentResult.IsSuccess)
        {
            return StatusCode(500, "Payment initiation failed: " + paymentResult?.Message);
        }

        // 5. Return Result (Redirect URL)
        return Ok(new { OrderId = order.Id, PaymentUrl = paymentResult.RedirectUrl });
    }
}

using Microsoft.AspNetCore.Mvc;
using DiscountManager.Modules.Basket.Domain;

namespace DiscountManager.Modules.Basket.Infrastructure;

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;

    public BasketController(IBasketRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<CustomerBasket>> GetBasket(Guid userId)
    {
        var basket = await _repository.GetBasketAsync(userId);
        return Ok(basket ?? new CustomerBasket(userId));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerBasket>> UpdateBasket([FromBody] CustomerBasket basket)
    {
        return Ok(await _repository.UpdateBasketAsync(basket));
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteBasket(Guid userId)
    {
        await _repository.DeleteBasketAsync(userId);
        return Ok();
    }
    
    [HttpPost("{userId}/items")]
    public async Task<ActionResult<CustomerBasket>> AddItem(Guid userId, [FromBody] BasketItem item)
    {
        var basket = await _repository.GetBasketAsync(userId) ?? new CustomerBasket(userId);
        
        var existingItem = basket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            basket.Items.Add(item);
        }
        
        return Ok(await _repository.UpdateBasketAsync(basket));
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<ActionResult<CustomerBasket>> RemoveItem(Guid userId, Guid productId)
    {
        var basket = await _repository.GetBasketAsync(userId);
        if (basket == null) return NotFound();

        var item = basket.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            basket.Items.Remove(item);
        }

        return Ok(await _repository.UpdateBasketAsync(basket));
    }
}

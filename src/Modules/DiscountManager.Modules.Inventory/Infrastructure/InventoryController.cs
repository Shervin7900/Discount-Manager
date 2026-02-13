using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Inventory.Infrastructure;

namespace DiscountManager.Modules.Inventory.Infrastructure;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _dbContext;

    public InventoryController(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetStock(Guid productId)
    {
        var inventory = await _dbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null)
        {
            return NotFound();
        }
        return Ok(new { productId = inventory.ProductId, quantity = inventory.Quantity });
    }

    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateStock(Guid productId, [FromBody] UpdateStockRequest request)
    {
        var inventory = await _dbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null)
        {
            return NotFound();
        }

        inventory.AdjustQuantity(request.Quantity - inventory.Quantity);
        await _dbContext.SaveChangesAsync();
        return Ok(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInventoryItem([FromBody] CreateInventoryRequest request)
    {
        var inventory = new Domain.InventoryItem(request.ProductId, request.Quantity);
        _dbContext.InventoryItems.Add(inventory);
        await _dbContext.SaveChangesAsync();
        return Ok(inventory);
    }
}

public record UpdateStockRequest(int Quantity);
public record CreateInventoryRequest(Guid ProductId, int Quantity);

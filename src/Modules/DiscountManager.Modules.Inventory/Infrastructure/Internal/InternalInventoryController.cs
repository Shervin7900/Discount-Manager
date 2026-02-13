using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Inventory.Infrastructure;

namespace DiscountManager.Modules.Inventory.Infrastructure.Internal;

/// <summary>
/// Internal API for inter-module communication (no authentication required)
/// </summary>
[ApiController]
[Route("internal/inventory")]
public class InternalInventoryController : ControllerBase
{
    private readonly InventoryDbContext _dbContext;

    public InternalInventoryController(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> CheckStock(Guid productId)
    {
        var inventory = await _dbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null)
        {
            return Ok(new { productId, quantity = 0, available = false });
        }
        
        return Ok(new 
        { 
            productId = inventory.ProductId,
            quantity = inventory.Quantity,
            available = inventory.Quantity > 0
        });
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockRequest request)
    {
        var inventory = await _dbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == request.ProductId);
        if (inventory == null)
        {
            return NotFound(new { message = "Product not found in inventory" });
        }

        if (inventory.Quantity < request.Quantity)
        {
            return BadRequest(new { message = "Insufficient stock", available = inventory.Quantity });
        }

        inventory.AdjustQuantity(-request.Quantity);
        await _dbContext.SaveChangesAsync();

        return Ok(new 
        { 
            productId = inventory.ProductId,
            reservedQuantity = request.Quantity,
            remainingQuantity = inventory.Quantity
        });
    }

    [HttpPost("release")]
    public async Task<IActionResult> ReleaseStock([FromBody] ReleaseStockRequest request)
    {
        var inventory = await _dbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == request.ProductId);
        if (inventory == null)
        {
            return NotFound(new { message = "Product not found in inventory" });
        }

        inventory.AdjustQuantity(request.Quantity);
        await _dbContext.SaveChangesAsync();

        return Ok(new 
        { 
            productId = inventory.ProductId,
            releasedQuantity = request.Quantity,
            newQuantity = inventory.Quantity
        });
    }

    [HttpPost("check-multiple")]
    public async Task<IActionResult> CheckMultipleStock([FromBody] CheckMultipleStockRequest request)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var inventoryItems = await _dbContext.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync();

        var results = request.Items.Select(item =>
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == item.ProductId);
            var available = inventory != null && inventory.Quantity >= item.Quantity;
            
            return new
            {
                productId = item.ProductId,
                requestedQuantity = item.Quantity,
                availableQuantity = inventory?.Quantity ?? 0,
                available
            };
        }).ToList();

        var allAvailable = results.All(r => r.available);

        return Ok(new
        {
            allAvailable,
            items = results
        });
    }
}

public record ReserveStockRequest(Guid ProductId, int Quantity);
public record ReleaseStockRequest(Guid ProductId, int Quantity);
public record CheckMultipleStockRequest(List<StockCheckItem> Items);
public record StockCheckItem(Guid ProductId, int Quantity);

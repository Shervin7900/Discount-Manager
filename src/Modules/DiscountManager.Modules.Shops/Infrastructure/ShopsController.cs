using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Shops.Infrastructure;

namespace DiscountManager.Modules.Shops.Infrastructure;

[ApiController]
[Route("api/shops")]
public class ShopsController : ControllerBase
{
    private readonly ShopsDbContext _dbContext;

    public ShopsController(ShopsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShops()
    {
        var shops = await _dbContext.Shops.ToListAsync();
        return Ok(shops);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShop(Guid id)
    {
        var shop = await _dbContext.Shops.FindAsync(id);
        if (shop == null)
        {
            return NotFound();
        }
        return Ok(shop);
    }

    [HttpGet("{shopId}/products")]
    [Authorize]
    public async Task<IActionResult> GetShopProducts(Guid shopId)
    {
        // This would typically call the Catalog module's internal API
        // For now, return a placeholder
        return Ok(new { shopId, message = "Products endpoint - requires Catalog module integration" });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopRequest request)
    {
        var shop = new Domain.Shop(request.Name, request.Address, request.Description, request.ImageUrl);
        _dbContext.Shops.Add(shop);
        await _dbContext.SaveChangesAsync();
        return Ok(shop);
    }
}

public record CreateShopRequest(string Name, string Address, string Description, string? ImageUrl);

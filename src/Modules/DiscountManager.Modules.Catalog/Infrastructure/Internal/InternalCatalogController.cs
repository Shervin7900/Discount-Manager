using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Catalog.Infrastructure;

namespace DiscountManager.Modules.Catalog.Infrastructure.Internal;

/// <summary>
/// Internal API for inter-module communication (no authentication required)
/// </summary>
[ApiController]
[Route("internal/catalog")]
public class InternalCatalogController : ControllerBase
{
    private readonly CatalogDbContext _dbContext;

    public InternalCatalogController(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound();
        }
        
        return Ok(new 
        { 
            id = product.Id,
            name = product.Name,
            description = product.Description,
            price = product.Price,
            salesPrice = product.SalesPrice,
            category = product.Category,
            shopId = product.ShopId
        });
    }

    [HttpPost("products/validate")]
    public async Task<IActionResult> ValidateProducts([FromBody] ValidateProductsRequest request)
    {
        var productIds = request.ProductIds;
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var validProducts = products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            price = p.SalesPrice ?? p.Price,
            isValid = true
        }).ToList();

        var invalidIds = productIds.Except(products.Select(p => p.Id)).ToList();
        var invalidProducts = invalidIds.Select(id => new
        {
            id,
            name = (string?)null,
            price = 0m,
            isValid = false
        });

        return Ok(new
        {
            validProducts,
            invalidProducts
        });
    }

    [HttpGet("products/shop/{shopId}")]
    public async Task<IActionResult> GetProductsByShop(Guid shopId)
    {
        var products = await _dbContext.Products
            .Where(p => p.ShopId == shopId)
            .ToListAsync();

        return Ok(products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            description = p.Description,
            price = p.Price,
            salesPrice = p.SalesPrice,
            category = p.Category
        }));
    }

    [HttpGet("products/active")]
    public async Task<IActionResult> GetActiveProducts()
    {
        var products = await _dbContext.Products.ToListAsync();
        return Ok(products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            price = p.Price,
            salesPrice = p.SalesPrice,
            category = p.Category
        }));
    }
}

public record ValidateProductsRequest(List<Guid> ProductIds);

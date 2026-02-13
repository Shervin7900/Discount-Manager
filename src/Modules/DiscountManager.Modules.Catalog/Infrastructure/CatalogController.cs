using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Catalog.Domain;

namespace DiscountManager.Modules.Catalog.Infrastructure;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly CatalogDbContext _dbContext;

    public CatalogController(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? category = null)
    {
        var query = _dbContext.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(x => x.Category == category);
        }
        var products = await query.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(string name, string description, decimal price, string category, Guid shopId)
    {
        var product = new Product(name, description, price, category, shopId);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return Ok(product);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Discount.Domain;

namespace DiscountManager.Modules.Discount.Infrastructure;

[ApiController]
[Route("api/discount")]
public class DiscountController : ControllerBase
{
    private readonly DiscountDbContext _dbContext;

    public DiscountController(DiscountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoupons()
    {
        var coupons = await _dbContext.Discounts.ToListAsync();
        return Ok(coupons);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCoupon(string code, decimal percentage, DateTime validFrom, DateTime validTo)
    {
        var coupon = new DiscountCoupon(code, percentage, validFrom, validTo);
        _dbContext.Discounts.Add(coupon);
        await _dbContext.SaveChangesAsync();
        return Ok(coupon);
    }
}

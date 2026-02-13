using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscountManager.Modules.Discount.Infrastructure;

namespace DiscountManager.Modules.Discount.Infrastructure.Internal;

/// <summary>
/// Internal API for inter-module communication (no authentication required)
/// </summary>
[ApiController]
[Route("internal/discount")]
public class InternalDiscountController : ControllerBase
{
    private readonly DiscountDbContext _dbContext;

    public InternalDiscountController(DiscountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("validate/{code}")]
    public async Task<IActionResult> ValidateCoupon(string code)
    {
        var discount = await _dbContext.Discounts
            .FirstOrDefaultAsync(d => d.Code == code);

        if (discount == null)
        {
            return Ok(new 
            { 
                code,
                isValid = false,
                message = "Coupon not found"
            });
        }

        var now = DateTime.UtcNow;
        var isActive = discount.ValidFrom <= now && discount.ValidTo >= now;

        return Ok(new
        {
            code = discount.Code,
            isValid = isActive,
            percentage = discount.Percentage,
            startDate = discount.ValidFrom,
            endDate = discount.ValidTo,
            message = isActive ? "Coupon is valid" : "Coupon has expired"
        });
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateDiscount([FromBody] CalculateDiscountRequest request)
    {
        var discount = await _dbContext.Discounts
            .FirstOrDefaultAsync(d => d.Code == request.CouponCode);

        if (discount == null)
        {
            return Ok(new
            {
                originalAmount = request.Amount,
                discountAmount = 0m,
                finalAmount = request.Amount,
                couponApplied = false,
                message = "Coupon not found"
            });
        }

        var now = DateTime.UtcNow;
        var isActive = discount.ValidFrom <= now && discount.ValidTo >= now;

        if (!isActive)
        {
            return Ok(new
            {
                originalAmount = request.Amount,
                discountAmount = 0m,
                finalAmount = request.Amount,
                couponApplied = false,
                message = "Coupon has expired"
            });
        }

        var discountAmount = request.Amount * (discount.Percentage / 100);
        var finalAmount = request.Amount - discountAmount;

        return Ok(new
        {
            originalAmount = request.Amount,
            discountAmount,
            finalAmount,
            couponApplied = true,
            percentage = discount.Percentage,
            message = $"{discount.Percentage}% discount applied"
        });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCoupons()
    {
        var now = DateTime.UtcNow;
        var activeCoupons = await _dbContext.Discounts
            .Where(d => d.ValidFrom <= now && d.ValidTo >= now)
            .ToListAsync();

        return Ok(activeCoupons.Select(d => new
        {
            code = d.Code,
            percentage = d.Percentage,
            startDate = d.ValidFrom,
            endDate = d.ValidTo
        }));
    }
}

public record CalculateDiscountRequest(string CouponCode, decimal Amount);

using FluentAssertions;
using DiscountManager.Modules.Discount.Domain;

namespace DiscountManager.Tests.Unit.Discount;

public class DiscountCouponTests
{
    [Fact]
    public void CreateCoupon_ShouldSetDates()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(7);

        // Act
        var coupon = new DiscountCoupon("SAVE10", 10, from, to);

        // Assert
        coupon.Code.Should().Be("SAVE10");
        coupon.ValidFrom.Should().Be(from);
        coupon.ValidTo.Should().Be(to);
    }
}

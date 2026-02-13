using Moq;
using FluentAssertions;
using DiscountManager.Modules.Basket.Infrastructure;
using DiscountManager.Modules.Basket.Domain;
using StackExchange.Redis;

namespace DiscountManager.Tests.Unit.Basket;

public class RedisBasketRepositoryTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;

    public RedisBasketRepositoryTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
    }

    [Fact]
    public async Task GetBasketAsync_WhenRedisIsDown_ShouldReturnFromFallback()
    {
        // Arrange
        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Throws(new Exception("Redis down"));
        var repository = new RedisBasketRepository(_mockRedis.Object);
        var userId = Guid.NewGuid();
        var basket = new CustomerBasket(userId);
        basket.Items.Add(new BasketItem { ProductId = Guid.NewGuid(), ProductName = "Test", UnitPrice = 10, Quantity = 1 });
        
        // Act: Seed fallback via Update
        await repository.UpdateBasketAsync(basket);
        
        // Assert: Get should now return it despite Redis being "down"
        var result = await repository.GetBasketAsync(userId);
        
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAndGet_WhenRedisIsWorking_ShouldPersistCorrectly()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        var repository = new RedisBasketRepository(_mockRedis.Object);
        var userId = Guid.NewGuid();
        var basket = new CustomerBasket(userId);

        // Act
        await repository.UpdateBasketAsync(basket);

        // Assert
        mockDb.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(k => k == $"basket:{userId}"), 
            It.IsAny<RedisValue>(), 
            null, 
            false, 
            When.Always, 
            CommandFlags.None), Times.AtLeastOnce);
    }
}

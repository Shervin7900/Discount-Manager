using Moq;
using FluentAssertions;
using DiscountManager.Modules.Search.Infrastructure;
using StackExchange.Redis;

namespace DiscountManager.Tests.Unit.Search;

public class RedisSearchServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;

    public RedisSearchServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Throws(new Exception("Redis down"));
    }

    [Fact]
    public async Task IndexAndSearch_WhenRedisIsDown_ShouldUseMemoryFallback()
    {
        // Arrange
        var service = new RedisSearchService(_mockRedis.Object);
        var productId = Guid.NewGuid();
        var name = "Gaming Keyboard";
        
        // Act
        await service.IndexProductAsync(productId, name, "Mechanical keyboard", 100, Guid.NewGuid(), false);
        var results = await service.SearchProductsAsync("Gaming");

        // Assert
        results.Should().Contain(productId);
    }

    [Fact]
    public async Task Search_WhenNoMatch_ShouldReturnEmpty()
    {
        // Arrange
        var service = new RedisSearchService(_mockRedis.Object);
        
        // Act
        var results = await service.SearchProductsAsync("NonExistent");

        // Assert
        results.Should().BeEmpty();
    }
}

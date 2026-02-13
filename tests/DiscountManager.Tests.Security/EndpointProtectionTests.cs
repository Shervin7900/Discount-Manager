using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace DiscountManager.Tests.Security;

public class EndpointProtectionTests : IClassFixture<WebApplicationFactory<DiscountManager.Bootstrapper.Program>>
{
    private readonly WebApplicationFactory<DiscountManager.Bootstrapper.Program> _factory;

    public EndpointProtectionTests(WebApplicationFactory<DiscountManager.Bootstrapper.Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/basket/080f303b-2717-4482-a039-9fc4506994ca", "GET")]
    [InlineData("/api/checkout/080f303b-2717-4482-a039-9fc4506994ca", "POST")]
    public async Task ProtectedEndpoints_ShouldReturn401Unauthorized_WhenAnonymous(string url, string method)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/catalog")]
    [InlineData("/api/shops")]
    [InlineData("/api/discount")]
    public async Task PublicEndpoints_ShouldReturnSuccess_WhenAnonymous(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-value");

        // Act
        var response = await client.GetAsync("/api/basket/080f303b-2717-4482-a039-9fc4506994ca");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

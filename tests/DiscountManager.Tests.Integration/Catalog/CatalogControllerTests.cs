using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using DiscountManager.Modules.Catalog.Domain;

namespace DiscountManager.Tests.Integration.Catalog;

public class CatalogControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CatalogControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/catalog");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_ShouldAddToDatabase()
    {
        // Arrange
        var client = _factory.CreateClient();
        var name = $"Test Product {Guid.NewGuid()}";
        
        // Act
        var response = await client.PostAsync($"/api/catalog?name={name}&description=Desc&price=50&category=Test&shopId={Guid.NewGuid()}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Name.Should().Be(name);
    }
}

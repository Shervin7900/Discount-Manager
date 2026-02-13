using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using DiscountManager.Modules.Catalog.Domain;

namespace DiscountManager.Tests.Security;

public class DatabaseSecurityTests : IClassFixture<WebApplicationFactory<DiscountManager.Bootstrapper.Program>>
{
    private readonly WebApplicationFactory<DiscountManager.Bootstrapper.Program> _factory;

    public DatabaseSecurityTests(WebApplicationFactory<DiscountManager.Bootstrapper.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CatalogSearch_ShouldBeResistantToSqlInjection()
    {
        // Arrange
        var client = _factory.CreateClient();
        var suspiciousInput = "Gaming' OR 1=1 --";

        // Act
        // This hits the internal search or catalog directly. 
        // We'll test catalog search if implemented, or just a general repo query.
        var response = await client.GetAsync($"/api/catalog?category={suspiciousInput}");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        
        // If SQL injection worked, it would return all products. 
        // Since it's parameterized, it should return 0 products as no category literally matches "Gaming' OR 1=1 --".
        products.Should().BeEmpty();
    }
}

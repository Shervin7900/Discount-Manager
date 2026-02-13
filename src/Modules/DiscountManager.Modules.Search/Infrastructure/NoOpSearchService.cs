namespace DiscountManager.Modules.Search.Infrastructure;

public class NoOpSearchService : ISearchService
{
    public Task IndexProductAsync(Guid productId, string name, string description, decimal price, Guid shopId, bool hasDiscount)
    {
        Console.WriteLine($"[NoOpSearch] Index request ignored for {name} (Redis unavailable).");
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Guid>> SearchProductsAsync(string query)
    {
        Console.WriteLine($"[NoOpSearch] Search request ignored (Redis unavailable).");
        return Task.FromResult(Enumerable.Empty<Guid>());
    }
}

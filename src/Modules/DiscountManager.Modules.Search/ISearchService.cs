namespace DiscountManager.Modules.Search;

public interface ISearchService
{
    Task IndexProductAsync(Guid productId, string name, string description, decimal price, Guid shopId, bool hasDiscount);
    Task<IEnumerable<Guid>> SearchProductsAsync(string query);
}

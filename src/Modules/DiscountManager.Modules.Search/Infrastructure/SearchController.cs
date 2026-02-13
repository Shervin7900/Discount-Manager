using Microsoft.AspNetCore.Mvc;

namespace DiscountManager.Modules.Search.Infrastructure;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? query)
    {
        var searchTerm = query ?? q ?? string.Empty;
        var results = await _searchService.SearchProductsAsync(searchTerm);
        return Ok(results);
    }
    
    [HttpPost("index")]
    public async Task<IActionResult> IndexProduct([FromBody] IndexProductRequest request)
    {
        await _searchService.IndexProductAsync(request.Id, request.Name, request.Description, request.Price, request.ShopId, request.HasDiscount);
        return Ok();
    }
}

public record IndexProductRequest(Guid Id, string Name, string Description, decimal Price, Guid ShopId, bool HasDiscount);

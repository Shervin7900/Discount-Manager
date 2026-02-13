using DiscountManager.Modules.Payment.Contracts;
using System.Text.Json;
using System.Net.Http.Json;

namespace DiscountManager.Modules.Ordering.Infrastructure.Clients;

public class BasketDto
{
    public Guid UserId { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
}

public class BasketItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public interface IBasketClient
{
    Task<BasketDto?> GetBasketAsync(Guid userId);
}

public interface IPaymentClient
{
    Task<PaymentResultDto?> RequestPaymentAsync(PaymentRequestDto request);
}

public class BasketClient : IBasketClient
{
    private readonly HttpClient _httpClient;

    public BasketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BasketDto?> GetBasketAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/basket/{userId}");
        if (!response.IsSuccessStatusCode) return null;
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BasketDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

public class PaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public PaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResultDto?> RequestPaymentAsync(PaymentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/payment/request", request);
        if (!response.IsSuccessStatusCode) return null;
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentResultDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

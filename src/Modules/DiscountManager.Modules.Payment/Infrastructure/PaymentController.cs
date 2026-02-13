using Microsoft.AspNetCore.Mvc;
using DiscountManager.Modules.Payment.Contracts;
using Parbad.AspNetCore;

namespace DiscountManager.Modules.Payment.Infrastructure;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestPayment([FromBody] PaymentRequestDto request)
    {
        var result = await _paymentService.RequestPaymentAsync(request);
        if (!result.IsSuccess) return BadRequest(result.Message);
        
        // In a real REST API, we might return the URL for the frontend to redirect
        // Or if using Parbad's auto-submit form:
        if (!string.IsNullOrEmpty(result.RedirectUrl))
        {
             return Ok(result);
        }
        
        // If it was some other form of transport, we'd handle it. 
        // For Virtual gateway, usually it gives a URL.
        return Ok(result);
    }
    
    [HttpGet("verify/{trackId}")]
    public async Task<IActionResult> VerifyPayment(string trackId)
    {
        var result = await _paymentService.VerifyPaymentAsync(trackId);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }
}

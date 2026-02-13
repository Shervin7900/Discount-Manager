using DiscountManager.Modules.Payment.Contracts;
using Parbad;
using Parbad.AspNetCore;
using Parbad.Gateway.ParbadVirtual;

namespace DiscountManager.Modules.Payment.Infrastructure;

public class PaymentService : IPaymentService
{
    private readonly IOnlinePayment _onlinePayment;

    public PaymentService(IOnlinePayment onlinePayment)
    {
        _onlinePayment = onlinePayment;
    }

    public async Task<PaymentResultDto> RequestPaymentAsync(PaymentRequestDto request)
    {
        var result = await _onlinePayment.RequestAsync(invoice =>
        {
            invoice
                .SetTrackingNumber(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0)) // Parbad requires long
                .SetAmount(request.Amount)
                .SetCallbackUrl(request.CallbackUrl)
                .UseParbadVirtual() // Use Virtual Gateway
                .SetGateway("ParbadVirtual");
        });

        if (result.IsSucceed)
        {
            // In a real app, you might might want to send the Transasction Code or Redirect URL
            // Parbad usually handles the redirect logic via Controller helper, but here we return URL if possible or just the result.
            // For Virtual gateway, we might need to construct the URL or let the controller handle 'ProcessResult'.
            
            // To be RESTful, we return the Redirect URL.
            // Parbad's RequestResult has generic GatewayTransporter.
            // We'll return the track ID mostly.
            
            return new PaymentResultDto(true, "Payment requested", result.GatewayTransporter.Descriptor.Url);
        }

        return new PaymentResultDto(false, result.Message, null);
    }

    public async Task<PaymentResultDto> VerifyPaymentAsync(string trackId)
    {
        // For verification, usually we get the token/trackId from the callback.
        long trackingNumber = long.Parse(trackId);
        
        var invoice = await _onlinePayment.FetchAsync(trackingNumber);
        
        if (invoice == null)
             return new PaymentResultDto(false, "Invoice not found", null);

        var verifyResult = await _onlinePayment.VerifyAsync(invoice);

        if (verifyResult.IsSucceed)
        {
            return new PaymentResultDto(true, "Payment Verified", null);
        }
        
        return new PaymentResultDto(false, verifyResult.Message, null);
    }
}

using DiscountManager.Modules.Payment.Contracts;

namespace DiscountManager.Modules.Payment;

public interface IPaymentService
{
    Task<PaymentResultDto> RequestPaymentAsync(PaymentRequestDto request);
    Task<PaymentResultDto> VerifyPaymentAsync(string trackId);
}

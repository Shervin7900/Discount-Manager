namespace DiscountManager.Modules.Payment.Contracts;

public record PaymentRequestDto(Guid OrderId, decimal Amount, string CallbackUrl);
public record PaymentResultDto(bool IsSuccess, string Message, string? RedirectUrl);

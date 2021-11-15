using Payment.Contracts;

namespace Payment.Server;

public interface IBankService
{
    Task<BankResponse> ProcessPayment(PaymentRequestCard card, PaymentAmount amount, PaymentCurrency currency);
}

public class BankResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

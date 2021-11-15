using Payment.Contracts;

namespace Payment.Server;

public class BankService : IBankService
{
    public Task<BankResponse> ProcessPayment(PaymentRequestCard card, PaymentAmount amount, PaymentCurrency currency)
    {
        if (card.Number.Value.StartsWith("5375"))
            return Task.FromResult(new BankResponse { Message = "master card not supported", Success = false });

        return Task.FromResult(new BankResponse { Success = true });
    }
}

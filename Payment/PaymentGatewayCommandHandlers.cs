using LanguageExt;
using LanguageExt.Common;
using MassTransit;
using Payment.Contracts;

namespace Payment;

public static class PaymentGatewayCommandHandlers
{
    public static string? OneTimeNextId { get; set; }
    public static string NextId
    {
        get
        {
            var result = OneTimeNextId ?? NewId.Next().ToString();
            OneTimeNextId = null;
            return result;
        }
    }
    public static Validation<Error, IEnumerable<PaymentGatewayEvents>> Handle(PaymentGatewayState state, ProcessPaymentRequestCommand command) =>
        from cardNumber in command.Card.Number.ValidatedValue
        from cardCVV in command.Card.CVV.ValidatedValue
        from cardExpiryMonth in command.Card.Expiry.Month.ValidatedValue
        from cardExpiryYear in command.Card.Expiry.Year.ValidatedValue
        from currency in command.Currency.ValidatedValue
        from amount in command.Amount.ValidatedValue
        from shopperId in command.ShopperId.ValidatedValue
        select new List<PaymentGatewayEvents> { new PaymentRequestProcessed(command.ShopperId, command.Card, command.Amount, command.Currency, new(NextId)) }.AsEnumerable();
}

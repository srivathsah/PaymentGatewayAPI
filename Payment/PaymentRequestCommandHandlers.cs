using LanguageExt.Common;
using Payment.Contracts;
using static LanguageExt.Prelude;

namespace Payment;

internal static class PaymentRequestCommandHandlers
{
    internal static LanguageExt.Validation<Error, IEnumerable<PaymentRequestEvents>> Handle(PaymentRequestState state, InitialisePaymentRequestCommand command) =>
        from cardNumber in command.Card.Number.ValidatedValue
        from cardCVV in command.Card.CVV.ValidatedValue
        from cardExpiryMonth in command.Card.Expiry.Month.ValidatedValue
        from cardExpiryYear in command.Card.Expiry.Year.ValidatedValue
        from currency in command.Currency.ValidatedValue
        from amount in command.Amount.ValidatedValue
        from merchant in command.MerchantId.ValidatedValue
        from shopper in command.ShopperId.ValidatedValue
        from status in state.Status is RequestStarted ? Success<Error, bool>(true) : Fail<Error, bool>(Error.New("Invalid Status"))
        select new List<PaymentRequestEvents> { new PaymentRequestInitialised(command.ShopperId, command.MerchantId, command.Card, command.Amount, command.Currency) }.AsEnumerable();

    internal static LanguageExt.Validation<Error, IEnumerable<PaymentRequestEvents>> Handle(PaymentRequestState state, AcceptPaymentRequestCommand command) =>
        from status in state.Status is ProcessStarted ? Success<Error, bool>(true) : Fail<Error, bool>(Error.New("Invalid Status"))
        select new List<PaymentRequestEvents> { new PaymentRequestAccepted() }.AsEnumerable();

    internal static LanguageExt.Validation<Error, IEnumerable<PaymentRequestEvents>> Handle(PaymentRequestState state, RejectPaymentRequestCommand command) =>
        from status in (state.Status is ProcessStarted) ? Success<Error, bool>(true) : Fail<Error, bool>(Error.New("Invalid Status"))
        select new List<PaymentRequestEvents> { new PaymentRequestRejected(command.Reason) }.AsEnumerable();
}

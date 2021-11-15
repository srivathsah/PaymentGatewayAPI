using Payment.Contracts;

namespace Payment;

internal static class PaymentRequestEventHandlers
{
    public static PaymentRequestState Apply(PaymentRequestState state, PaymentRequestInitialised evt) =>
        state with
        {
            Amount = evt.Amount,
            Card = evt.Card,
            Currency = evt.Currency,
            ShopperId = evt.ShopperId,
            MerchantId = evt.MerchantId,
            Status = new ProcessStarted()
        };

    public static PaymentRequestState Apply(PaymentRequestState state, PaymentRequestAccepted evt) =>
        state with { Status = new AcceptedByBank() };

    public static PaymentRequestState Apply(PaymentRequestState state, PaymentRequestRejected evt) =>
        state with { Status = new RejectedByBank(evt.Reason) };
}

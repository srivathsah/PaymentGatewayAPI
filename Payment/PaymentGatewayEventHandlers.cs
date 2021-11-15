using Payment.Contracts;

namespace Payment;

internal static class PaymentGatewayEventHandlers
{
    public static PaymentGatewayState Apply(PaymentGatewayState state, PaymentRequestProcessed evt) =>
        state with { NumberOfPaymentsProcessed = state.NumberOfPaymentsProcessed + 1 };
}

using Domain;
using LanguageExt;
using LanguageExt.Common;
using Payment.Contracts;
using static Payment.PaymentGatewayCommandHandlers;
using static Payment.PaymentGatewayEventHandlers;

namespace Payment;

public record PaymentGatewayState(int NumberOfPaymentsProcessed);

public class PaymentGateway : Aggregate<PaymentGatewayId, PaymentGatewayState, PaymentGatewayCommands, PaymentGatewayEvents>
{
    public PaymentGateway(PaymentGatewayId id) : base(id)
    {
    }

    public override PaymentGatewayState ApplyEvent(PaymentGatewayState state, PaymentGatewayEvents evt) =>
        evt switch
        {
            PaymentRequestProcessed paymentRequestProcessed => Apply(state, paymentRequestProcessed),
            _ => state
        };

    public override Validation<Error, IEnumerable<PaymentGatewayEvents>> Execute(PaymentGatewayState state, PaymentGatewayCommands command) =>
        command switch
        {
            ProcessPaymentRequestCommand paymentRequestCommand => Handle(state, paymentRequestCommand),
            _ => Error.New("Invalid Command")
        };
}

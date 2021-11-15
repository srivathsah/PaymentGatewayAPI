using Domain;
using LanguageExt;
using LanguageExt.Common;
using Payment.Contracts;
using static Payment.PaymentRequestCommandHandlers;
using static Payment.PaymentRequestEventHandlers;

namespace Payment;

public record PaymentRequestState(
                             ShopperId ShopperId,
                             MerchantId MerchantId,
                             PaymentRequestCard Card,
                             PaymentAmount Amount,
                             PaymentCurrency Currency,
                             PaymentRequestStatus Status)
{
    public static PaymentRequestState Init() =>
        new(ShopperId.Init(), MerchantId.Init(), PaymentRequestCard.Init(), PaymentAmount.Init(), PaymentCurrency.Init(), new RequestStarted());
}

public class PaymentRequest : Aggregate<PaymentRequestId, PaymentRequestState, PaymentRequestCommands, PaymentRequestEvents>
{
    public PaymentRequest(PaymentRequestId id) : base(id)
    {
    }

    public override PaymentRequestState ApplyEvent(PaymentRequestState state, PaymentRequestEvents evt) =>
        evt switch
        {
            PaymentRequestInitialised paymentRequestInitialised => Apply(state, paymentRequestInitialised),
            PaymentRequestAccepted paymentRequestAccepted => Apply(state, paymentRequestAccepted),
            PaymentRequestRejected paymentRequestRejected => Apply(state, paymentRequestRejected),
            _ => state
        };

    public override Validation<Error, IEnumerable<PaymentRequestEvents>> Execute(PaymentRequestState state, PaymentRequestCommands command) =>
        command switch
        {
            InitialisePaymentRequestCommand initialisePaymentRequestCommand => Handle(state, initialisePaymentRequestCommand),
            AcceptPaymentRequestCommand acceptPaymentRequestCommand => Handle(state, acceptPaymentRequestCommand),
            RejectPaymentRequestCommand rejectPaymentRequestCommand => Handle(state, rejectPaymentRequestCommand),
            _ => Error.New("Invalid Command")
        };
}

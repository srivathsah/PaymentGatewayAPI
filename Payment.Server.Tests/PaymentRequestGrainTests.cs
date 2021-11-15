using Payment.Client;
using Payment.Contracts;
using Xunit;

namespace Payment.Server.Tests;

[Collection(ClusterCollection.Name)]
public class PaymentRequestGrainTests : GrainTestsBase<IPaymentRequestClient, PaymentRequestId, PaymentRequestState, PaymentRequestCommands, PaymentRequestEvents>
{
    public PaymentRequestGrainTests(ClusterFixture fixture) : base(fixture)
    {
        ValidCommandFactory = () => new InitialisePaymentRequestCommand(ShopperId.Init(), MerchantId.Init(), PaymentRequestCard.Init() with { CVV = new(010) }, new(10), PaymentCurrency.Init());
        InvalidCommandFactory = () => new RejectPaymentRequestCommand(new(""));
    }
}

using Payment.Client;
using Payment.Contracts;
using Xunit;

namespace Payment.Server.Tests;

[Collection(ClusterCollection.Name)]
public class PaymentGatewayGrainTests : GrainTestsBase<IPaymentGatewayClient, PaymentGatewayId, PaymentGatewayState, PaymentGatewayCommands, PaymentGatewayEvents>
{
    public PaymentGatewayGrainTests(ClusterFixture fixture) : base(fixture)
    {
        ValidCommandFactory = () => new ProcessPaymentRequestCommand(ShopperId.Init(), PaymentRequestCard.Init() with { CVV = new(010) }, new(10), PaymentCurrency.Init());
        InvalidCommandFactory = () => new ProcessPaymentRequestCommand(ShopperId.Init(), PaymentRequestCard.Init(), PaymentAmount.Init(), new(""));
    }
}

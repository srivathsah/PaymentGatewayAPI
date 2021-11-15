using Alba;
using Moq;
using Orleans.Concurrency;
using Payment.Contracts;
using Xunit;

namespace Payment.API.Tests;

public class PaymentGatewayIntegrationTests : IntegrationContext
{
    public PaymentGatewayIntegrationTests(AppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public Task ProcessPaymentShouldSucceedWithValidModel()
    {
        var input = new ProcessPaymentRequestCommand(ShopperId.Init(), PaymentRequestCard.Init() with { CVV = new(010) }, new(10), PaymentCurrency.Init());
        FakeClusterClientFactory.MerchantServiceMock.Setup(x => x.GetMerchantIntegerId(It.IsAny<string>())).ReturnsAsync(1);
        FakeClusterClientFactory.PaymentGatewayClientMock.Setup(x => x.Execute(new Immutable<PaymentGatewayCommands>(input), 1))
            .ReturnsAsync(Success<PaymentGatewayEvents>());

        return Scenario(_ =>
        {
            _.Post.Json(input).ToUrl($"/api/paymentgateway/process/");
            _.StatusCodeShouldBeOk();
        });
    }

    [Fact]
    public Task GetTransactionsShouldSucceed()
    {
        FakeClusterClientFactory.MerchantServiceMock.Setup(x => x.GetMerchantIntegerId(It.IsAny<string>())).ReturnsAsync(1);
        FakeClusterClientFactory.MerchantServiceMock.Setup(x => x.GetAllTransactions(It.IsAny<int>())).ReturnsAsync(new { });

        return Scenario(_ =>
        {
            _.Get.Url($"/api/paymentgateway/transactions/");
            _.StatusCodeShouldBeOk();
        });
    }
}

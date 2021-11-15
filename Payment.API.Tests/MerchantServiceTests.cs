using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace Payment.API.Tests;

public class MerchantServiceTests
{
    [Fact]
    public async Task GetAllTransactionsShouldWorkAsExpected()
    {
        var clientHandlerMock = new Mock<DelegatingHandler>();
        clientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();
        clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());

        var httpClient = new HttpClient(clientHandlerMock.Object);

        var clientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        clientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();

        var merchantService = new MerchantService(clientFactoryMock.Object);
        await merchantService.GetAllTransactions(1);

        clientFactoryMock.VerifyAll();
        clientHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}

using ClusterUtils;
using Moq;
using Orleans;
using Payment.Client;

namespace Payment.API.Tests;

public class FakeClusterClientFactory : IClusterClientFactory
{
    private readonly Mock<IClusterClient> _mock = new(MockBehavior.Strict);
    public static readonly Mock<IPaymentGatewayClient> PaymentGatewayClientMock = new();
    public static readonly Mock<IPaymentRequestClient> PaymentRequestClientMock = new();
    public static readonly Mock<IMerchantService> MerchantServiceMock = new(MockBehavior.Strict);

    public Task<IClusterClient> GetClusterClient(int clusterPort, string cluster, string service, Action<ClientBuilder>? builderAction = null)
    {
        _mock.Setup(x => x.GetGrain<IPaymentGatewayClient>(It.IsAny<string>(), null)).Returns(PaymentGatewayClientMock.Object);
        _mock.Setup(x => x.GetGrain<IPaymentRequestClient>(It.IsAny<string>(), null)).Returns(PaymentRequestClientMock.Object);
        _mock.Setup(x => x.Connect(null)).Returns(Task.CompletedTask);
        _mock.Setup(x => x.Dispose()).Verifiable();
        return Task.FromResult(_mock.Object);
    }
}

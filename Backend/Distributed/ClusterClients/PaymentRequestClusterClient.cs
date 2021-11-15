using Backend.Contracts;
using ClusterUtils;
using Payment.Client;
using Payment.Contracts;

namespace Backend.Distributed.ClusterClients;

public class PaymentRequestClusterClient : ClusterClientBase<IPaymentRequestClient, PaymentRequestId, PaymentRequestCommands, PaymentRequestEvents>,
    IPaymentRequestBackendClient
{
    public PaymentRequestClusterClient(IClusterClientFactory clusterClientFactory)
        : base(25004, "paymentRequestCluster", "paymentRequest-service1", clusterClientFactory) { }
}

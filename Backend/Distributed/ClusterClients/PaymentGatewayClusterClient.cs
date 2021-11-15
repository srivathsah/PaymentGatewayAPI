using Backend.Contracts;
using ClusterUtils;
using Payment.Client;
using Payment.Contracts;

namespace Backend.Distributed.ClusterClients;

public class PaymentGatewayClusterClient : ClusterClientBase<IPaymentGatewayClient, PaymentGatewayId, PaymentGatewayCommands, PaymentGatewayEvents>,
    IPaymentGatewayBackendCllient
{
    public PaymentGatewayClusterClient(IClusterClientFactory clusterClientFactory)
        : base(15004, "paymentsCluster", "paymentsCluster-service1", clusterClientFactory) { }
}

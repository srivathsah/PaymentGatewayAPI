using Backend.Contracts;
using Backend.Distributed.ClusterClients;
using ClusterUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Distributed
{
    public class DistributedBackendProvider : IBackendProvider
    {
        private readonly PaymentGatewayClusterClient _paymentGatewayClusterClient;

        public DistributedBackendProvider(IClusterClientFactory clusterClientFactory)
        {
            _paymentGatewayClusterClient = new(clusterClientFactory);
        }

        public IServiceCollection AddBackend(IServiceCollection services)
        {
            services.AddSingleton<IPaymentGatewayBackendCllient>(_paymentGatewayClusterClient);
            return services;
        }

        public async Task Initialize(IServiceProvider serviceProvider)
        {
            await _paymentGatewayClusterClient.TryInit();
        }
    }
}

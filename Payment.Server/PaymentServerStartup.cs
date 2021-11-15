using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Payment.Client;
using ServerUtils;

namespace Payment.Server;

public class PaymentServerStartup : OrleansServerStartup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton<IBankService, BankService>();
    }

    public override async Task OnOrleansStarted(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.OnOrleansStarted(serviceProvider, cancellationToken);
        IClusterClient? clusterClient = serviceProvider.GetService<IClusterClient>();
        if (clusterClient is not null)
        {
            await clusterClient.GetGrain<IPaymentEventsConsumer>("CommittedEventsConsumer").StartConsumption();
            await clusterClient.GetGrain<IPaymentExternalEventsPublisher>("ExternalCommittedEventsPublisher").StartAsync(cancellationToken);
        }
    }

    public override ISiloBuilder BeforeOrleansBuild(ISiloBuilder siloBuilder)
    {
        siloBuilder = base.BeforeOrleansBuild(siloBuilder);
        siloBuilder.ConfigureApplicationParts(parts =>
        {
            parts.AddApplicationPart(typeof(IPaymentGatewayClient).Assembly);
        });
        return siloBuilder;
    }
}

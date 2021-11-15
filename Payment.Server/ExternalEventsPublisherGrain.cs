using MassTransit;
using ServerUtils;

namespace Payment.Server;

internal class ExternalEventsPublisherGrain : RabbitMQHostedService, IPaymentExternalEventsPublisher
{
    public ExternalEventsPublisherGrain(IBusControl bus) : base(bus)
    {
    }
}

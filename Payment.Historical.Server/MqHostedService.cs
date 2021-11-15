using MassTransit;

namespace Payment.Historical.Server;

public class MqHostedService : IExternalEventsPublisher
{
    private readonly IBusControl _bus;
    public MqHostedService(IBusControl bus) => _bus = bus;
    public Task Publish<T>(T obj) where T : class => _bus.Publish(obj);
    public async Task ExecuteAsync(CancellationToken stoppingToken) => await _bus.StartAsync(stoppingToken).ConfigureAwait(false);
}

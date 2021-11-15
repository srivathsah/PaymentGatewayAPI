using Backend.Shared;
using Domain.EventSourcing;
using MassTransit;
using Orleans;
using Orleans.Streams;
using System.Reactive.Linq;

namespace ServerUtils;

public abstract class RabbitMQHostedService : Grain, IExternalCommittedEventsPublisher
{
    private readonly IBusControl _bus;
    private IStreamProvider? _streamProvider;
    private IAsyncStream<CommittedEvent>? _asyncStream;
    private StreamSubscriptionHandle<CommittedEvent>? _committedEventsSubscription;
    private bool _started;

    public RabbitMQHostedService(IBusControl bus) => _bus = bus;

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        _streamProvider = GetStreamProvider("ORLEANS_STREAM_PROVIDER");
        if (_asyncStream == null)
        {
            _asyncStream = _streamProvider.GetStream<CommittedEvent>($"CommittedEvents".ToGuid(), "Global");
            _committedEventsSubscription = await _asyncStream.SubscribeAsync((ce, sst) => Publish(ce));
        }
    }

    public virtual async Task Publish(CommittedEvent committedEvent) =>
        await _bus.Publish(committedEvent.DomainEvent, c =>
        {
            c.Headers.Set("MerchantId", committedEvent.MerchantId);
            c.Headers.Set("SenderId", committedEvent.SenderId);
        });

    public virtual Task Publish<T>(T obj) where T : class => _bus.Publish(obj);

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_started)
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
        _started = true;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _started = false;
        return _bus.StopAsync(cancellationToken);
    }
}

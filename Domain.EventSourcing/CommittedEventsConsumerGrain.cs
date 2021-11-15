using Backend.Shared;
using Orleans;
using Orleans.Streams;

namespace Domain.EventSourcing;

public abstract class CommittedEventsConsumerGrain : Grain, ICommittedEventsConsumerGrain
{
    private IStreamProvider? _streamProvider;
    private IAsyncStream<CommittedEvent>? _asyncStream;
    private string? _MerchantId;
    private StreamSubscriptionHandle<CommittedEvent>? _committedEventsSubscription;

    public override Task OnActivateAsync()
    {
        var result = base.OnActivateAsync();
        _streamProvider = GetStreamProvider("ORLEANS_STREAM_PROVIDER");
        _MerchantId = this.GetPrimaryKeyString().Split(new char[] { '-' }).Last();
        return result;
    }

    public async Task StartConsumption()
    {
        if (_asyncStream == null)
        {
            _asyncStream = _streamProvider?.GetStream<CommittedEvent>($"CommittedEvents".ToGuid(), "Global");
            if (_asyncStream != null)
                _committedEventsSubscription = await _asyncStream.SubscribeAsync((ce, sst) => Consume(ce));
        }
    }

    public abstract Task Consume(CommittedEvent committedEvent);
}

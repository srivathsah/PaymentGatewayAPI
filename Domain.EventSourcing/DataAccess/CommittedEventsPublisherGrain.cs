using Backend.Shared;
using Orleans;
using Orleans.Streams;
using Serialization;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace Domain.EventSourcing.DataAccess;

public class CommittedEventsPublisherGrain : Grain, IInternalCommittedEventsPublisher
{
    private readonly IStreamStore _streamStore;
    private readonly ISerializer _serializer;
    private IAllStreamSubscription? _allStreamSubscription;

    private IStreamProvider? _streamProvider;
    private IAsyncStream<CommittedEvent>? _asyncStream;

    public CommittedEventsPublisherGrain(IStreamStore streamStore, ISerializer serializer)
    {
        _streamStore = streamStore;
        _serializer = serializer;
    }

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        _streamProvider = GetStreamProvider("ORLEANS_STREAM_PROVIDER");
        _asyncStream = _streamProvider?.GetStream<CommittedEvent>($"CommittedEvents".ToGuid(), $"Global");
    }

    public async Task Start()
    {
        if (_allStreamSubscription == null)
        {
            var latestId = await _streamStore.ReadHeadPosition();
            _allStreamSubscription = _streamStore.SubscribeToAll(latestId, StreamMessageReceived, SubscriptionDropped);
        }
    }

    public Task Stop()
    {
        _allStreamSubscription?.Dispose();
        return Task.CompletedTask;
    }

    private async Task StreamMessageReceived(IAllStreamSubscription subscription, StreamMessage streamMessage, CancellationToken cancellationtoken)
    {
        var json = await streamMessage.GetJsonData(cancellationtoken);
        var metadata = _serializer.DeserializeObject<EventMetadata>(streamMessage.JsonMetadata);
        var eventObject = _serializer.DeserializeObject(json);
        if (_asyncStream != null && eventObject != null)
            await _asyncStream.OnNextAsync(new CommittedEvent { DomainEvent = eventObject, MerchantId = metadata?.OrgId, SenderId = streamMessage.StreamId });
    }

    private void SubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception)
    {
        // React to the SubscriptionDroppedReason such as re-establish a subscription
    }
}

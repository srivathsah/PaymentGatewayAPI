using Backend.Shared;
using MassTransit;
using Microsoft.Extensions.Logging;
using Serialization;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Domain.EventSourcing.DataAccess;

public class StreamStoreEventsDataAccess : IEventDataAccess
{
    private readonly IStreamStore _streamStore;
    private readonly ISerializer _serializer;
    private readonly ILogger<StreamStoreEventsDataAccess> _logger;

    public StreamStoreEventsDataAccess(
        IStreamStore streamStore,
        ISerializer serializer,
        ILogger<StreamStoreEventsDataAccess> logger
        )
    {
        _streamStore = streamStore;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task SaveEvent(string aggregateType, object aggregateId, object obj, int MerchantId) =>
        await _streamStore.AppendToStream(
            aggregateId.ToString(), ExpectedVersion.Any,
            new NewStreamMessage(
                NewId.NextGuid(), aggregateType,
                _serializer.SerializeObject(obj),
                _serializer.SerializeObject(new EventMetadata { OrgId = new MerchantId { Value = MerchantId } })));

    public async Task FetchHistoricalEvents(object aggregateId, Action<object> action)
    {
        var endOfStream = false;
        var startVersion = 0;
        while (!endOfStream)
        {
            var stream = await _streamStore.ReadStreamForwards(aggregateId.ToString(), startVersion, 50);
            endOfStream = stream.IsEnd;
            startVersion = stream.NextStreamVersion;

            foreach (var msg in stream.Messages)
            {
                var json = await msg.GetJsonData();
                var obj = _serializer.DeserializeObject(json);
                if (obj != null)
                    action(obj);
            }
        }
    }
}

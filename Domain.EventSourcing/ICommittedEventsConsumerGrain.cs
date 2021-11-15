using Backend.Shared;
using Orleans;

namespace Domain.EventSourcing;

public interface ICommittedEventsConsumerGrain : ICommittedEventsConsumer, IGrainWithStringKey
{
}

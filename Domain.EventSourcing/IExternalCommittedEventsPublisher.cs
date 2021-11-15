using Backend.Shared;
using Orleans;

namespace Domain.EventSourcing;

public interface IExternalCommittedEventsPublisher : ICommittedEventsPublisher, IGrainWithStringKey
{
}

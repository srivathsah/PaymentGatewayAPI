using Orleans;

namespace Domain.EventSourcing.DataAccess;

public interface IInternalCommittedEventsPublisher : IGrainWithStringKey
{
    Task Start();
    Task Stop();
}

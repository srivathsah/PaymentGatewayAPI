namespace Backend.Shared;

public interface ICommittedEventsConsumer
{
    Task StartConsumption();
    Task Consume(CommittedEvent committedEvent);
}

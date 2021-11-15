namespace Backend.Shared;

public interface ICommittedEventsPublisher
{
    Task Publish(CommittedEvent committedEvent);
    Task Publish<T>(T obj) where T : class;
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

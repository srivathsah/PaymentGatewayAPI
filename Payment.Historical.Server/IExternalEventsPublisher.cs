namespace Payment.Historical.Server;

public interface IExternalEventsPublisher
{
    Task Publish<T>(T obj) where T : class;
    Task ExecuteAsync(CancellationToken stoppingToken);
}

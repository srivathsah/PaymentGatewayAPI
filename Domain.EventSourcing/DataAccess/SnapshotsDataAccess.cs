using Microsoft.Extensions.Logging;

namespace Domain.EventSourcing.DataAccess;

public class SnapshotsDataAccess : ISnapshotDataAccess
{
    private readonly ILogger<SnapshotsDataAccess> _logger;

    public SnapshotsDataAccess(ILogger<SnapshotsDataAccess> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetSnapshot<T>(string id)
    {
        _logger.LogInformation($"GetSnapshot {id}");
        return Task.FromResult(default(T));
    }

    public Task SaveSnasphot<T>(T item)
    {
        throw new NotImplementedException();
    }
}

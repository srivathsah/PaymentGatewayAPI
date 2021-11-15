namespace Domain.EventSourcing.DataAccess;

public interface ISnapshotDataAccess
{
    public Task SaveSnasphot<T>(T item);
    public Task<T?> GetSnapshot<T>(string id);
}

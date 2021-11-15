namespace Backend.Shared;

public interface IEventDataAccess
{
    Task SaveEvent(string aggregateType, object aggregateId, object eventObject, int MerchantId);
    Task FetchHistoricalEvents(object aggregateId, Action<object> action);
}

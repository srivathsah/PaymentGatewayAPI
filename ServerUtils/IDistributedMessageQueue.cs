using Backend.Shared;

namespace ServerUtils;

public interface IDistributedMessageQueue
{
    Task Publish<T>(T @event, MerchantId MerchantId);
}

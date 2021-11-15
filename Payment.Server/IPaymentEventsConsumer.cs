using Domain.EventSourcing;

namespace Payment.Server;

public interface IPaymentEventsConsumer : ICommittedEventsConsumerGrain
{
}

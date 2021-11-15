using Domain.EventSourcing;

namespace Payment.Server;

public interface IPaymentExternalEventsPublisher : IExternalCommittedEventsPublisher
{
}

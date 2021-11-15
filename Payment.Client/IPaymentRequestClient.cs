using Domain.EventSourcing;
using Payment.Contracts;

namespace Payment.Client;

public interface IPaymentRequestClient : IDomainClient<PaymentRequestId, PaymentRequestCommands, PaymentRequestEvents>
{
}

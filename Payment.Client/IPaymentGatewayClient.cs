using Domain.EventSourcing;
using Payment.Contracts;

namespace Payment.Client;

public interface IPaymentGatewayClient : IDomainClient<PaymentGatewayId, PaymentGatewayCommands, PaymentGatewayEvents>
{
}

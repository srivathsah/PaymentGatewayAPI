using Backend.Shared;
using Payment.Contracts;

namespace Backend.Contracts;

public interface IPaymentGatewayBackendCllient : IBackendClient<PaymentGatewayId, PaymentGatewayCommands, PaymentGatewayEvents>
{
}

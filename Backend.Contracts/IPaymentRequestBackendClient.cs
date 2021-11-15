using Backend.Shared;
using Payment.Contracts;

namespace Backend.Contracts;

public interface IPaymentRequestBackendClient : IBackendClient<PaymentRequestId, PaymentRequestCommands, PaymentRequestEvents>
{
}

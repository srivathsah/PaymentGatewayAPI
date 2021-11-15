using Backend.Shared;
using Domain.EventSourcing;
using Domain.EventSourcing.DataAccess;
using Payment.Client;
using Payment.Contracts;

namespace Payment.Server;

public class PaymentGatewayGrain : EventSourcedDomainGrain<PaymentGateway, PaymentGatewayId, PaymentGatewayState, PaymentGatewayCommands, PaymentGatewayEvents>, IPaymentGatewayClient
{
    private static readonly Func<PaymentGatewayState> _defaultStateFactory = () => new(0);
    private static readonly Func<string, PaymentGateway> _paymentGatewayFactory = (id) => new(new(id));

    public PaymentGatewayGrain(ISnapshotDataAccess snapshotDataAccess, IEventDataAccess eventDataAccess) :
        base(_paymentGatewayFactory, _defaultStateFactory, snapshotDataAccess, eventDataAccess)
    {
    }
}

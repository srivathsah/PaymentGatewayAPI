using Backend.Shared;
using Domain.EventSourcing;
using Domain.EventSourcing.DataAccess;
using Payment.Client;
using Payment.Contracts;

namespace Payment.Server;

internal class PaymentRequestGrain : EventSourcedDomainGrain<PaymentRequest, PaymentRequestId, PaymentRequestState, PaymentRequestCommands, PaymentRequestEvents>, IPaymentRequestClient
{
    private static readonly Func<PaymentRequestState> _defaultStateFactory = () => PaymentRequestState.Init();
    private static readonly Func<string, PaymentRequest> _PaymentRequestFactory = (id) => new(new(id));

    public PaymentRequestGrain(ISnapshotDataAccess snapshotDataAccess, IEventDataAccess eventDataAccess) :
        base(_PaymentRequestFactory, _defaultStateFactory, snapshotDataAccess, eventDataAccess)
    {
    }
}

using Payment.Contracts;
using ServerUtils;

namespace Payment.Historical.Server;

public class PaymentRequestInitialisedConsumer : MessageConsumerBase<PaymentRequestInitialised>
{
    private readonly IHistoricalService _historicalService;

    public PaymentRequestInitialisedConsumer(IHistoricalService historicalService)
    {
        _historicalService = historicalService;
    }

    public override async Task Consume(PaymentRequestInitialised message, int MerchantId, string senderId)
    {
        await _historicalService.OnPaymentRequestInitialised(message, senderId);
    }
}

public class PaymentRequestAcceptedConsumer : MessageConsumerBase<PaymentRequestAccepted>
{
    private readonly IHistoricalService _historicalService;

    public PaymentRequestAcceptedConsumer(IHistoricalService historicalService)
    {
        _historicalService = historicalService;
    }

    public override async Task Consume(PaymentRequestAccepted message, int MerchantId, string senderId)
    {
        await _historicalService.OnPaymentRequestAccepted(message, senderId);
    }
}

public class PaymentRequestRejectedConsumer : MessageConsumerBase<PaymentRequestRejected>
{
    private readonly IHistoricalService _historicalService;

    public PaymentRequestRejectedConsumer(IHistoricalService historicalService)
    {
        _historicalService = historicalService;
    }

    public override async Task Consume(PaymentRequestRejected message, int MerchantId, string senderId)
    {
        await _historicalService.OnPaymentRequestRejected(message, senderId);
    }
}

using Backend.Shared;
using Domain.EventSourcing;
using Orleans.Concurrency;
using Payment.Client;
using Payment.Contracts;

namespace Payment.Server;

internal class InternalEventsConsumerGrain : CommittedEventsConsumerGrain, IPaymentEventsConsumer
{
    private readonly IBankService _bankService;

    public InternalEventsConsumerGrain(IBankService bankService)
    {
        _bankService = bankService;
    }

    public override Task Consume(CommittedEvent committedEvent) =>
    committedEvent.DomainEvent switch
    {
        PaymentRequestProcessed paymentRequestProcessed when committedEvent.MerchantId is not null
            => OnPaymentRequestProcessed(paymentRequestProcessed, committedEvent.MerchantId.Value),
        PaymentRequestInitialised paymentRequestInitialised when committedEvent.MerchantId is not null =>
            OnPaymentRequestInitialised(paymentRequestInitialised, committedEvent.MerchantId.Value, committedEvent.SenderId ?? ""),
        _ => Task.CompletedTask

    };

    private async Task OnPaymentRequestInitialised(PaymentRequestInitialised paymentRequestInitialised, int merchantId, string senderId)
    {
        var response = await _bankService.ProcessPayment(paymentRequestInitialised.Card, paymentRequestInitialised.Amount, paymentRequestInitialised.Currency);
        if (response.Success)
        {
            await GrainFactory.GetGrain<IPaymentRequestClient>($"{senderId}").Execute(new Immutable<PaymentRequestCommands>(new AcceptPaymentRequestCommand()), merchantId);
        }
        else
        {
            await GrainFactory.GetGrain<IPaymentRequestClient>($"{senderId}").Execute(new Immutable<PaymentRequestCommands>(new RejectPaymentRequestCommand(new(response.Message))), merchantId);
        }
    }


    private async Task OnPaymentRequestProcessed(PaymentRequestProcessed paymentRequestProcessed, int merchantId) =>
        await GrainFactory
        .GetGrain<IPaymentRequestClient>($"{paymentRequestProcessed.Id.Value}")
        .Execute(new Immutable<PaymentRequestCommands>(
            new InitialisePaymentRequestCommand(paymentRequestProcessed.ShopperId,
                                                new(merchantId.ToString()),
                                                paymentRequestProcessed.Card,
                                                paymentRequestProcessed.Amount,
                                                paymentRequestProcessed.Currency)
         ), merchantId);
}

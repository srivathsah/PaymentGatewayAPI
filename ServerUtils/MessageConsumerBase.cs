using Backend.Shared;
using MassTransit;

namespace ServerUtils;

public abstract class MessageConsumerBase<T> : IConsumer<T> where T : class
{
    public virtual Task Consume(ConsumeContext<T> context) =>
        Consume(context.Message, GetMerchantId(context), GetSenderId(context));

    public abstract Task Consume(T message, int MerchantId, string senderId);

    private static int GetMerchantId(ConsumeContext context) =>
        context.Headers.Get<MerchantId>(nameof(MerchantId))?.Value ?? -2;

    private static string GetSenderId(ConsumeContext context) =>
        context.Headers.Get<string>("SenderId") ?? "";
}

using LanguageExt;
using LanguageExt.Common;
using Orleans;
using Orleans.Concurrency;

namespace Domain.EventSourcing;

public interface IDomainClient<TId, TCommand, TEvent> : IGrainWithStringKey
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TId : ValueRecord<string>
{
    Task Initialise();
    Task<Immutable<Validation<Error, IEnumerable<TEvent>>>> Execute(Immutable<TCommand> command, int MerchantId);
}

using Domain;
using LanguageExt;
using LanguageExt.Common;

namespace Backend.Shared;

public interface IBackendClient<TId, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TId : ValueRecord<string>
{
    Task TryInit();
    Task<Validation<Error, IEnumerable<TEvent>>> Execute(TId id, TCommand command, int MerchantId);
}

using LanguageExt;
using LanguageExt.Common;

namespace Domain;

public abstract class Aggregate<TIdentity, TState, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
{
    public Aggregate(TIdentity id) => Id = id;

    public TIdentity Id { get; }

    public abstract Validation<Error, IEnumerable<TEvent>> Execute(TState state, TCommand command);

    public TState ApplyEvents(TState state, IEnumerable<TEvent> events) =>
        events.Aggregate(state, (state, @event) => ApplyEvent(state, @event));

    public abstract TState ApplyEvent(TState state, TEvent @evt);
}

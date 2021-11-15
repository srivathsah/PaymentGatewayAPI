using Backend.Shared;
using Domain.EventSourcing.DataAccess;
using LanguageExt;
using LanguageExt.Common;
using Orleans;
using Orleans.Concurrency;

namespace Domain.EventSourcing;

public abstract class EventSourcedDomainGrain<TAggregate, TId, TState, TCommand, TEvent> : Grain, IDomainClient<TId, TCommand, TEvent>
    where TAggregate : Aggregate<TId, TState, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TState : class
    where TId : ValueRecord<string>
{
    private readonly Func<string, TAggregate> _aggregateFactory;
    private readonly Func<TState> _defaultStateFactory;
    private readonly IEventDataAccess _eventDataAccess;
    private readonly ISnapshotDataAccess _snapshotDataAccess;

    protected TAggregate? Aggregate { get; set; }
    protected string? IDString { get; private set; }
    protected TState? State { get; set; }
    protected int? MerchantId { get; set; }
    protected bool StateRehydrated { get; set; }

    public EventSourcedDomainGrain(
        Func<string, TAggregate> aggregateFactory,
        Func<TState> defaultStateFactory,
        ISnapshotDataAccess snapshotDataAccess,
        IEventDataAccess eventsDataAccess
        )
    {
        _aggregateFactory = aggregateFactory;
        _defaultStateFactory = defaultStateFactory;
        _snapshotDataAccess = snapshotDataAccess;
        _eventDataAccess = eventsDataAccess;
    }

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        IDString = this.GetPrimaryKeyString();
        Aggregate = _aggregateFactory(IDString);
        TState? state = (await _snapshotDataAccess.GetSnapshot<TState>(IDString));
        State = state ?? _defaultStateFactory();
        if (state == null)
        {
            State = await RehydrateStateFromSourcedEvents(State);
            StateRehydrated = true;
        }
    }

    protected virtual async Task<Validation<Error, IEnumerable<TEvent>>> ExecuteCommandAndPublishEvents(TCommand command, int merchantId)
    {
        if (Aggregate == null)
        {
            return Error.New("AggreagteIsNull");
        }

        if (State == null)
        {
            return Error.New("StateIsNull");
        }

        if (IDString == null)
        {
            return Error.New("IDIsNull");
        }

        if (MerchantId == null) MerchantId = merchantId;
        var domainCommandResult = Aggregate.Execute(State, command);
        await domainCommandResult.Match(async val =>
        {
            State = Aggregate.ApplyEvents(State, val.OfType<TEvent>());
            foreach (var @event in val)
            {
                await _eventDataAccess.SaveEvent(@event.GetType().FullName, IDString, @event, merchantId);
                await OnEventCommitted(@event);
            }
        }, err => Task.CompletedTask);

        return domainCommandResult;
    }

    protected virtual Task OnEventCommitted(DomainEvent @event) => Task.CompletedTask;

    public virtual async Task<Immutable<Validation<Error, IEnumerable<TEvent>>>> Execute(Immutable<TCommand> command, int MerchantId) =>
        new Immutable<Validation<Error, IEnumerable<TEvent>>>(await ExecuteCommandAndPublishEvents(command.Value, MerchantId));

    public virtual async Task<TState> RehydrateStateFromSourcedEvents(TState state)
    {
        if (Aggregate == null) return state;
        if (IDString == null) return state;
        await _eventDataAccess.FetchHistoricalEvents(IDString, @evt =>
        {
            state = (@evt is TEvent @event) ? Aggregate.ApplyEvent(state, @event) : state;
        }).ConfigureAwait(false);
        return state;
    }

    public virtual Task Initialise() => Task.CompletedTask;
}

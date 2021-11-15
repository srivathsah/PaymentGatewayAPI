using Backend.Shared;
using Domain.EventSourcing.DataAccess;
using Orleans;

namespace Domain.EventSourcing;

public abstract class EventSourcedGrain<TState, TCommand, TEvent> : Grain
    where TState : class
{
    private readonly string _type;
    private readonly Func<TState> _defaultStateFactory;
    private readonly IEventDataAccess _eventDataAccess;
    private readonly Func<TCommand, List<TEvent>> _doFunc;
    private readonly ISnapshotDataAccess _snapshotDataAccess;

    public EventSourcedGrain(
        string type,
        Func<TState> defaultStateFactory,
        ISnapshotDataAccess snapshotDataAccess,
        IEventDataAccess eventsDataAccess,
        Func<TCommand, List<TEvent>> doFunc
        )
    {
        _type = type;
        _defaultStateFactory = defaultStateFactory;
        _snapshotDataAccess = snapshotDataAccess;
        _eventDataAccess = eventsDataAccess;
        _doFunc = doFunc;
    }

    protected string? IDString { get; private set; }
    protected TState? State { get; private set; }
    protected Action<TEvent>? OnHistoricalEventFetched { get; set; }
    protected Action? PreDo { get; set; }
    protected Action<TEvent>? PostApply { get; set; }

    public virtual async Task<TState> RehydrateStateFromSourcedEvents(TState state)
    {
        if (IDString != null && OnHistoricalEventFetched != null)
        {
            await _eventDataAccess.FetchHistoricalEvents(IDString, evt =>
            {
                if (evt is TEvent @event)
                    OnHistoricalEventFetched(@event);
            }).ConfigureAwait(false);

            return State ?? state;
        }

        return state;
    }

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        IDString = this.GetPrimaryKeyString();
        TState? state = (await _snapshotDataAccess.GetSnapshot<TState>(IDString));
        State = state ?? _defaultStateFactory();
        if (state == null)
        {
            State = await RehydrateStateFromSourcedEvents(State);
        }
    }

    protected virtual async Task DoAndSave(TCommand command, int orgid)
    {
        PreDo?.Invoke();
        var events = _doFunc(command);
        if (events != null)
        {
            var tasks = events.Select(@evt => evt != null ? _eventDataAccess.SaveEvent(_type, this.GetPrimaryKeyString(), @evt, orgid) : Task.CompletedTask);
            await Task.WhenAll(tasks);
            events.ForEach(@evt => PostApply?.Invoke(@evt));
        }
    }

    protected void ChangeState(TState state)
    {
        State = state;
    }
}

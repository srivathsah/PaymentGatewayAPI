using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EventBus;

public class RxEventBus : IEventBus
{
    protected Subject<object> Subject { get; } = new Subject<object>();
    protected IConnectableObservable<object> ConnectableObservable { get; }

    public RxEventBus()
    {
        ConnectableObservable = Subject.ObserveOn(Scheduler.Default).Publish();
        ConnectableObservable.Connect();
    }

    public virtual void Publish<T>(T @event)
    {
        if (@event != null)
            Subject.OnNext(@event);
    }

    public virtual IObservable<T> GetSubscription<T>()
    {
        return ConnectableObservable.OfType<T>();
    }

    public virtual void Stop()
    {
        Subject.OnCompleted();
    }
}

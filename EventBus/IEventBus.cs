using System;

namespace EventBus;

public interface IEventBus
{
    void Publish<T>(T @event);
    void Stop();
    IObservable<T> GetSubscription<T>();
}

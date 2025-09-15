using System;

namespace Events
{
    public interface IEventBus
    {
        IObservable<T> OnEvent<T>();
        void Publish<T>(T evt);
    }
}
using System;

namespace DefaultNamespace.EventBus
{
    public interface IEventBus
    {
        IObservable<T> OnEvent<T>();
        void Publish<T>(T evt);
    }
}
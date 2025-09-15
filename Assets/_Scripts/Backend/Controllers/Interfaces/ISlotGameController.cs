using System;
using UniRx;

namespace Controllers.Interfaces
{
    public interface ISlotGameController
    {
        IObservable<Unit> OnGameInitialized { get; }
        void Initialize();
    }
}
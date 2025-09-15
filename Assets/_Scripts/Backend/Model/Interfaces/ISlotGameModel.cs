using System;
using Cysharp.Threading.Tasks;

namespace _Scripts.Model
{
    public interface ISlotGameModel
    {
        event Action<ISpinResult> OnSpinCompleted;
        event Action OnSpinStarted;
        
        bool CanSpin { get; }
        
        void Initialize();
        UniTask<ISpinResult> PerformSpinAsync();
    }
}
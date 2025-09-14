using System;
using _Scripts.Model;
using UniRx;

namespace Views
{
    public interface ISlotMachineView
    {
        IObservable<Unit> OnSpinRequested { get; }
        
        void SetSpinButtonEnabled(bool enabled);
        void ShowMessage(string message);
        void UpdatePayoutDisplay(int payout);
        void StartReelAnimations();
        void StopReelAnimations(ISlotSymbol[,] results);
        void ShowWinEffects();
        void HideWinEffects();
        void HighlightWinningSymbols(int[] winningPositions);
        void ClearSymbolHighlights();
    }
}
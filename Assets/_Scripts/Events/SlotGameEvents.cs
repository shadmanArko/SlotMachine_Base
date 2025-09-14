using System;
using _Scripts.Model;

namespace Controllers.Events
{
    public class SlotGameEvents
    {
        public static event Action GameInitialized;
        public static event Action SpinRequested;
        public static event Action SpinStarted;
        public static event Action<ISpinResult> SpinCompleted;
        
        // UI Events
        public static event Action<ISpinResult> ShowSpinResult;
        public static event Action<string> ShowMessage;
        public static event Action<int> UpdatePayout;
        
        
        // Animation Events
        public static event Action StartReelAnimations;
        public static event Action<ISlotSymbol[,]> StopReelAnimations;
        public static event Action ShowWinEffects;
        public static event Action HideWinEffects;
        
        // Error Events
        public static event Action<string> OnError;

        // Event Triggers
        public static void TriggerGameInitialized() => GameInitialized?.Invoke();
        public static void TriggerSpinRequested() => SpinRequested?.Invoke();
        public static void TriggerSpinStarted() => SpinStarted?.Invoke();
        public static void TriggerSpinCompleted(ISpinResult result) => SpinCompleted?.Invoke(result);
        public static void TriggerShowSpinResult(ISpinResult result) => ShowSpinResult?.Invoke(result);
        public static void TriggerShowMessage(string message) => ShowMessage?.Invoke(message);
        public static void TriggerUpdatePayout(int payout) => UpdatePayout?.Invoke(payout);
        public static void TriggerStartReelAnimations() => StartReelAnimations?.Invoke();
        public static void TriggerStopReelAnimations(ISlotSymbol[,] results) => StopReelAnimations?.Invoke(results);
        public static void TriggerShowWinEffects() => ShowWinEffects?.Invoke();
        public static void TriggerHideWinEffects() => HideWinEffects?.Invoke();
        public static void TriggerError(string error) => OnError?.Invoke(error);

        // Cleanup method for scene transitions
        public static void ClearAllEvents()
        {
            GameInitialized = null;
            SpinRequested = null;
            SpinStarted = null;
            SpinCompleted = null;
            ShowSpinResult = null;
            ShowMessage = null;
            UpdatePayout = null;
            StartReelAnimations = null;
            StopReelAnimations = null;
            ShowWinEffects = null;
            HideWinEffects = null;
            OnError = null;
        }
    }
}
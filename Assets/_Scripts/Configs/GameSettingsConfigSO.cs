using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "SlotGame/Game Settings")]
    public class GameSettingsConfigSO : ScriptableObject
    {
        [Header("UI Settings")]
        [SerializeField] private bool enableWinAnimations = true;
        [SerializeField] private bool enableSoundEffects = true;
        [SerializeField] private float winDisplayDuration = 3f;
        
        [Header("Performance Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableVSync = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool showPaylineDebugInfo = false;

        public bool EnableWinAnimations => enableWinAnimations;
        public bool EnableSoundEffects => enableSoundEffects;
        public float WinDisplayDuration => winDisplayDuration;
        public int TargetFrameRate => targetFrameRate;
        public bool EnableVSync => enableVSync;
        public bool EnableDebugLogs => enableDebugLogs;
        public bool ShowPaylineDebugInfo => showPaylineDebugInfo;
    }
}
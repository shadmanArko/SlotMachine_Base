#if UNITY_EDITOR
using Configs;
using UnityEngine;
using UnityEditor;

namespace SlotGame.Editor
{
    public static class SlotGameMenuItems
    {
        [MenuItem("SlotGame/Create Default Configuration")]
        public static void CreateDefaultConfiguration()
        {
            var config = ScriptableObject.CreateInstance<SlotConfigurationSO>();
            
            // Set default values programmatically
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Slot Configuration",
                "DefaultSlotConfig",
                "asset",
                "Choose location to save the slot configuration");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = config;
            }
        }

        [MenuItem("SlotGame/Create Game Settings")]
        public static void CreateGameSettings()
        {
            var settings = ScriptableObject.CreateInstance<GameSettingsConfigSO>();
            
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Game Settings",
                "GameSettings",
                "asset",
                "Choose location to save the game settings");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = settings;
            }
        }

        [MenuItem("SlotGame/Setup Scene")]
        public static void SetupScene()
        {
            // Create basic scene setup
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Create SceneContext for Zenject
            var sceneContext = Object.FindObjectOfType<Zenject.SceneContext>();
            if (sceneContext == null)
            {
                var contextGO = new GameObject("SceneContext");
                sceneContext = contextGO.AddComponent<Zenject.SceneContext>();
            }

            Debug.Log("Scene setup completed!");
        }
    }
}
#endif
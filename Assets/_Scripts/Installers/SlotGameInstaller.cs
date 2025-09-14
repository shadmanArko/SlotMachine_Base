using _Scripts.Model;
using Configs;
using Controllers;
using DefaultNamespace.EventBus;
using UnityEngine;
using Views;
using Zenject;

namespace Installers
{ 
    [CreateAssetMenu(fileName = "SlotGameInstaller", menuName = "SlotGame/SlotGameInstaller")]
    public class SlotGameInstaller : ScriptableObjectInstaller<SlotGameInstaller>
    {
        [Header("Configurations")]
        [SerializeField] private SlotConfigurationSO slotConfiguration;
        [SerializeField] private GameSettingsConfigSO gameSettings;

        [Header("View References")]
        [SerializeField] private SlotMachineView slotMachineView;

        public override void InstallBindings()
        {
            InstallConfigurations();
            InstallServices();
            InstallModels();
            InstallControllers();
            InstallViews();
        }

        private void InstallConfigurations()
        {
            Container.BindInterfacesAndSelfTo<SlotConfigurationSO>().FromScriptableObject(slotConfiguration).AsSingle();
            Container.Bind<GameSettingsConfigSO>().FromScriptableObject(gameSettings).AsSingle();
        }

        private void InstallServices()
        {
            Container.BindInterfacesTo<SystemRandomProvider>().AsSingle();
            Container.BindInterfacesTo<UniRxEventBus>().AsSingle();
        }

        private void InstallModels()
        {
            Container.BindInterfacesTo<SlotGameModel>().AsSingle();
        }

        private void InstallControllers()
        {
            Container.BindInterfacesTo<SlotGameController>().AsSingle();
        }

        private void InstallViews()
        {
            Container.BindInterfacesTo<SlotMachineView>().FromComponentInNewPrefab(slotMachineView).AsSingle();
        }

        private void OnValidate()
        {
            if (slotConfiguration == null)
            {
                Debug.LogWarning("SlotConfiguration is not assigned in SlotGameInstaller!");
            }

            if (gameSettings == null)
            {
                Debug.LogWarning("GameSettings is not assigned in SlotGameInstaller!");
            }

            if (slotMachineView == null)
            {
                Debug.LogWarning("SlotMachineView is not assigned in SlotGameInstaller!");
            }
        }
    }
}
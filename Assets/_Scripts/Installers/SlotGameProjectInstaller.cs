using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(fileName = "SlotGameProjectSettings", menuName = "SlotGame/Project Settings")]
    public class SlotGameProjectInstaller : ScriptableObjectInstaller<SlotGameProjectInstaller>
    {
        public override void InstallBindings()
        {
            InstallProjectServices();
            InstallEventSystem();
        }

        private void InstallProjectServices()
        {
            // Project-wide services can be installed here
            // For example: Audio manager, Analytics, etc.
        }

        private void InstallEventSystem()
        {
            // Event system is static, but we can initialize it here if needed
            Container.Bind<IInitializable>()
                .To<EventSystemInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }

    public class EventSystemInitializer : IInitializable
    {
        public void Initialize()
        {
            // Initialize event system if needed
            // SlotGameEvents are static, so no initialization required
        }
    }
}
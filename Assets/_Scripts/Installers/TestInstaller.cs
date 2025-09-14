#if UNITY_EDITOR || DEVELOPMENT_BUILD
using _Scripts.Model;
using DefaultNamespace;
using Zenject;

namespace SlotGame.Installers
{
    public class TestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Override random provider for testing
            Container.Bind<IRandomProvider>()
                .To<MockRandomProvider>()
                .AsSingle()
                .WhenInjectedInto<SlotGameModel>();
        }
    }

    public class MockRandomProvider : IRandomProvider
    {
        private readonly int[] _predefinedValues;
        private int _currentIndex;

        public MockRandomProvider(params int[] values)
        {
            _predefinedValues = values ?? new int[] { 0 };
            _currentIndex = 0;
        }

        public int Next(int maxValue)
        {
            var value = _predefinedValues[_currentIndex % _predefinedValues.Length];
            _currentIndex++;
            return value % maxValue;
        }

        public int Next(int minValue, int maxValue)
        {
            var value = Next(maxValue - minValue);
            return minValue + value;
        }

        public float NextFloat()
        {
            return Next(1000) / 1000f;
        }
    }
}
#endif
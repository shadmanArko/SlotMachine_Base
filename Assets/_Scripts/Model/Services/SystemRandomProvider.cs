using System;

namespace _Scripts.Model
{
    public class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public int Next(int maxValue) => _random.Next(maxValue);
        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        public float NextFloat() => (float)_random.NextDouble();
    }
}
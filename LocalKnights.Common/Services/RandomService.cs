using System;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Default implementation of <see cref="IRandomService"/>.
    /// Seeds the random number generator using the current timestamp.
    /// </summary>
    public class RandomService : IRandomService
    {
        private readonly Random _random;
        private readonly TimeProvider _timeProvider;
        private readonly object _lock = new object();

        public RandomService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;

            // Seed using current timestamp so behavior is deterministic per run but different across runs
            int seed = unchecked((int)_timeProvider.GetUtcNow().Ticks);
            _random = new Random(seed);
        }

        public int Next()
        {
            lock (_lock)
            {
                return _random.Next();
            }
        }

        public int Next(int maxValue)
        {
            lock (_lock)
            {
                return _random.Next(maxValue);
            }
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            lock (_lock)
            {
                return _random.Next(minValue, maxValue);
            }
        }

        /// <inheritdoc/>
        public double NextDouble()
        {
            lock (_lock)
            {
                return _random.NextDouble();
            }
        }

        /// <inheritdoc/>
        public double NextDouble(double minValue, double maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            lock (_lock)
            {
                return minValue + _random.NextDouble() * (maxValue - minValue);
            }
        }

        /// <inheritdoc/>
        public bool NextBool()
        {
            lock (_lock)
            {
                return _random.Next(0, 2) == 1;
            }
        }
    }
}

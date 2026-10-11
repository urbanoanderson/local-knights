using System;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Interface for generating random values. Implementations should be mockable for tests.
    /// </summary>
    public interface IRandomService
    {
        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        int Next();

        /// <summary>
        /// Returns a non-negative random integer that is less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be returned. Must be greater than zero.</param>
        int Next(int maxValue);

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Returns a random double in the range [0.0, 1.0).
        /// </summary>
        double NextDouble();

        /// <summary>
        /// Returns a random double within the specified range.
        /// </summary>
        /// <param name="minValue">Inclusive lower bound.</param>
        /// <param name="maxValue">Exclusive upper bound.</param>
        double NextDouble(double minValue, double maxValue);

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        bool NextBool();
    }
}

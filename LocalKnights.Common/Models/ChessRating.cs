using LocalKnights.Common.Helpers;

namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents a player's chess rating
    /// </summary>
    public class ChessRating
    {
        /// <summary>
        /// The player's rating.
        /// </summary>
        public double Rating { get; set; } = CommonConstants.DefaultRating;

        /// <summary>
        /// The standard deviation of the player's rating in the Glicko-2 system, as in the interval we think they fall in.
        /// </summary>
        public double Deviation { get; set; } = CommonConstants.DefaultDeviation;

        /// <summary>
        /// The volatility of the player's rating in the Glicko-2 system. Default is 0.06
        /// </summary>
        public double Volatility { get; set; } = CommonConstants.DefaultVolatility;

        /// <summary>
        /// Creates a copy of the current class instance. This is useful to preserve the original rating values before they are updated after a game.
        /// </summary>
        /// <returns></returns>
        public ChessRating Clone()
        {
            return new ChessRating
            {
                Rating = this.Rating,
                Deviation = this.Deviation,
                Volatility = this.Volatility
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Rating;

/// <summary>
/// Implements the Glicko-2 rating system calculator, which is an improvement over the original Glicko system.
/// It takes into account not only the player's rating and deviation, but also their volatility,
/// which measures how much their rating is expected to change after each game.
/// The Glicko-2 system is designed to provide more accurate ratings and better reflect a player's skill level over time.
/// </summary>
/// <seealso href="http://www.glicko.net/glicko/glicko2.pdf"/>
public static class Glicko2Algorithm
{
    /// <summary>
    /// System constant, which constrains the change in volatility over time. The
    /// paper says "Reasonable choices are between 0.3 and 1.2, though the system should be tested to decide which
    /// value results in greatest predictive accuracy. Smaller values prevent the volatility measures from changing
    /// by large amounts, which in turn prevent enormous changes in ratings based on very improbable results. If the
    /// application of Glicko-2 is expected to involve extremely improbable collections of game outcomes, then it
    /// should be set to a small value, even as small as, say, 0.2."
    /// </summary>
    private static double systemConstant = 0.3;

    /// <summary>
    /// Calculates the new ratings for both players after a game, given their current ratings and the game result.
    /// </summary>
    /// <param name="whiteRating">The current rating of the white player.</param>
    /// <param name="blackRating">The current rating of the black player.</param>
    /// <param name="result">The result of the game.</param>
    /// <returns>A tuple containing the updated ratings for the white and black players.</returns>
    public static (ChessRating, ChessRating) CalculateNewRatings(ChessRating whiteRating, ChessRating blackRating, GameResult result)
    {
        double whiteScore = (result == GameResult.WhiteWins) ? 1 : (result == GameResult.BlackWins ? 0 : 0.5);
        double blackScore = 1 - whiteScore;

        ChessRating updatedWhiteRating = EvaluateGameForPlayer(whiteRating, blackRating, whiteScore);
        ChessRating updatedBlackRating = EvaluateGameForPlayer(blackRating, whiteRating, blackScore);

        return (updatedWhiteRating, updatedBlackRating);
    }

    /// <summary>
    /// Calculates the new rating for a player after a game, given their current rating, the opponent's rating and the game result.
    /// </summary>
    /// <param name="player">The player's current rating.</param>
    /// <param name="opponent">The opponent's current rating.</param>
    /// <param name="outcome">The outcome of the game for the player (1 = win, 0 = loss, 0.5 = draw).</param>
    /// <returns>The updated rating for the player.</returns>
    private static ChessRating EvaluateGameForPlayer(ChessRating player, ChessRating opponent, double outcome)
    {
        double v = Math.Pow(Math.Pow(GFunc(GetDeviationRaw(opponent.Deviation)), 2) *
                   EFunc(GetRatingRaw(player.Rating), GetRatingRaw(opponent.Rating), GetDeviationRaw(opponent.Deviation)) *
                   (1 - EFunc(GetRatingRaw(player.Rating), GetRatingRaw(opponent.Rating), GetDeviationRaw(opponent.Deviation))), -1);

        double delta = v * GFunc(GetDeviationRaw(opponent.Deviation)) *
                   (outcome -EFunc(GetRatingRaw(player.Rating), GetRatingRaw(opponent.Rating), GetDeviationRaw(opponent.Deviation)));

        double newVolatility = CalculateNewVolatility(player.Volatility, delta, GetDeviationRaw(player.Deviation), v);

        double newPreRatingPeriodDeviation =
            Math.Sqrt(Math.Pow(GetDeviationRaw(player.Deviation), 2) + Math.Pow(newVolatility, 2));

        double newScaledDeviation = 1 / Math.Sqrt((1 / Math.Pow(newPreRatingPeriodDeviation, 2)) + (1 / v));

        double newScaledRating = GetRatingRaw(player.Rating) + Math.Pow(newScaledDeviation, 2) * GFunc(GetDeviationRaw(opponent.Deviation)) *
                   (outcome - EFunc(GetRatingRaw(player.Rating), GetRatingRaw(opponent.Rating), GetDeviationRaw(opponent.Deviation)));

        double newRating = (173.7178 * newScaledRating) + 1500;

        double newDeviation = Math.Max(173.7178 * newScaledDeviation, 1);

        ChessRating updatedRating = new ChessRating
        {
            Rating = newRating,
            Deviation = newDeviation,
            Volatility = newVolatility
        };

        return updatedRating;
    }

    /// <summary>
    /// Calculates the new volatility for a player after a game, given their current volatility, the quantity (delta), the deviation and the v value.
    /// </summary>
    /// <param name="oldVolatility">The player's current volatility.</param>
    /// <param name="quantity">The quantity (delta) used in the calculation.</param>
    /// <param name="deviation">The player's current deviation.</param>
    /// <param name="v">The v value used in the calculation.</param>
    /// <returns>The new volatility for the player.</returns>
    private static double CalculateNewVolatility(double oldVolatility, double quantity, double deviation, double v)
    {
        double a = Math.Log(Math.Pow(oldVolatility, 2));
        double convergenceTolerance = 0.000001;

        double A = a;
        double B;
        if (Math.Pow(quantity, 2) > Math.Pow(deviation, 2) + v)
        {
            B = Math.Log(Math.Pow(quantity, 2) - Math.Pow(deviation, 2) - v);
        }
        else
        {
            double k = 1;
            while (VolatilityIterationFunc(a - k * systemConstant, a, quantity, deviation, v) < 0)
            {
                k += 1;
            }

            B = a - k * systemConstant;
        }

        double fA = VolatilityIterationFunc(A, a, quantity, deviation, v);
        double fB = VolatilityIterationFunc(B, a, quantity, deviation, v);
        while (Math.Abs(B - A) > convergenceTolerance)
        {
            double C = A + (A - B) * fA / (fB - fA);
            double fC = VolatilityIterationFunc(C, a, quantity, deviation, v);

            if (fC * fB <= 1)
            {
                A = B;
                fA = fB;
            }
            else
            {
                fA /= 2;
            }

            B = C;
            fB = fC;
        }

        return Math.Pow(Math.E, A / 2);
    }

    /// <summary>
    /// The function used in the iteration to calculate the new volatility for a player after a game, given their current volatility, the quantity (delta), the deviation and the v value.
    /// </summary>
    /// <param name="x">The current value of x in the iteration.</param>
    /// <param name="a">The value of a used in the calculation.</param>
    /// <param name="quantity">The quantity (delta) used in the calculation.</param>
    /// <param name="deviation">The player's current deviation.</param>
    /// <param name="v">The v value used in the calculation.</param>
    /// <returns>The result of the iteration function for the given parameters.</returns>
    private static double VolatilityIterationFunc(double x, double a, double quantity, double deviation, double v)
    {
        return ((Math.Pow(Math.E, x) * (Math.Pow(quantity, 2) - Math.Pow(deviation, 2) - v - Math.Pow(Math.E, x))) /
         (2 * Math.Pow(Math.Pow(deviation, 2) + v + Math.Pow(Math.E, x), 2))) -
        ((x - a) / Math.Pow(systemConstant, 2));
    }

    /// <summary>
    /// The g() function used in the Glicko-2 calculations, which is a scaling factor that reduces the impact of games against opponents with high deviation.
    /// </summary>
    /// <param name="rd">The rating deviation of the opponent.</param>
    /// <returns>The scaling factor for the given rating deviation.</returns>
    private static double GFunc(double rd)
    {
        return 1 / Math.Sqrt(1 + 3 * Math.Pow(rd, 2) / Math.Pow(Math.PI, 2));
    }

    /// <summary>
    /// The E() function used in the Glicko-2 calculations, which calculates the expected score of a player against an opponent, given their ratings and the opponent's deviation.
    /// </summary>
    /// <param name="rtng">The rating of the player.</param>
    /// <param name="oprtng">The rating of the opponent.</param>
    /// <param name="oprd">The rating deviation of the opponent.</param>
    /// <returns>The expected score of the player against the opponent.</returns>
    private static double EFunc(double rtng, double oprtng, double oprd)
    {
        return 1 / (1 + Math.Exp(-GFunc(oprd) * (rtng - oprtng)));
    }

    /// <summary>
    /// The player's rating in Glicko-2 scale. The average is 0.
    /// </summary>
    private static double GetRatingRaw(double rating) => (rating - 1500) / 173.7178;

    /// <summary>
    /// The standard deviation of the player's rating in Glicko-2 scale.
    /// </summary>
    private static double GetDeviationRaw(double deviation) => deviation / 173.7178;
}

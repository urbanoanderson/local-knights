using System;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Rating;

/// <summary>
/// Implements the ELO rating system calculator.
/// The ELO system is a method for calculating the relative skill levels of players in zero-sum games such as chess.
/// It is named after its creator Arpad Elo, and is used by FIDE, the international chess federation.
/// </summary>
public static class EloAlgorithm
{
    /// <summary>
    /// Calculates the new ratings for both players after a game using the ELO rating system.
    /// </summary>
    /// <param name="whiteRating">The current rating of the white player.</param>
    /// <param name="blackRating">The current rating of the black player.</param>
    /// <param name="result">The result of the game.</param>
    /// <param name="whiteKFactor">The K-factor for the white player.</param>
    /// <param name="blackKFactor">The K-factor for the black player.</param>
    /// <returns>A tuple containing the new ratings for white and black players.</returns>
    public static (ChessRating whiteNewRating, ChessRating blackNewRating) CalculateNewRatings(
        ChessRating whiteRating,
        ChessRating blackRating,
        GameResult result,
        int whiteKFactor,
        int blackKFactor)
    {
        // Determine actual scores based on game result
        double whiteScore = (result == GameResult.WhiteWins) ? 1 : (result == GameResult.BlackWins ? 0 : 0.5);
        double blackScore = 1 - whiteScore;

        // Calculate expected scores
        double whiteExpected = 1.0 / (1.0 + Math.Pow(10, (blackRating.Rating - whiteRating.Rating) / 400.0));
        double blackExpected = 1.0 - whiteExpected;

        // Calculate new ratings
        double whiteNewRatingValue = whiteRating.Rating + whiteKFactor * (whiteScore - whiteExpected);
        double blackNewRatingValue = blackRating.Rating + blackKFactor * (blackScore - blackExpected);

        // Create new ChessRating objects with updated ratings, keeping deviation and volatility unchanged
        ChessRating whiteNewRating = new ChessRating
        {
            Rating = whiteNewRatingValue,
            Deviation = whiteRating.Deviation,
            Volatility = whiteRating.Volatility
        };

        ChessRating blackNewRating = new ChessRating
        {
            Rating = blackNewRatingValue,
            Deviation = blackRating.Deviation,
            Volatility = blackRating.Volatility
        };

        return (whiteNewRating, blackNewRating);
    }

    /// <summary>
    /// Determines the appropriate K-factor for a player based on the number of games they have played in the last year and their current rating. The K-factor is a constant that determines how much a player's rating changes after a game. A higher K-factor results in larger rating changes, while a lower K-factor results in smaller rating changes. This method allows for dynamic adjustment of the K-factor based on player activity and skill level, providing a more accurate and responsive rating system.
    /// </summary>
    /// <param name="gamesPlayedLastYear">The number of games the player has played in the last year.</param>
    /// <param name="provisionalGamesSetting">The number of games a player is considered provisional in the ELO system, which can be set in the club preferences.</param>
    /// <param name="playerRating">The current rating of the player within the time format.</param>
    public static int GetKFactorBasedOnNumberOfGames(int gamesPlayedLastYear, int provisionalGamesSetting, ChessRating playerRating)
    {
        /*
        For players with less than the provisional games setting within last year,
        we use a higher K-factor to account for greater uncertainty in their rating.
        However, if their rating is very high (e.g., 2400 or above),
        we can use a slightly lower K-factor to prevent excessive rating changes from a single game,
        as they are likely already at an established skill level.
        */
        if (gamesPlayedLastYear < provisionalGamesSetting)
        {
            if (playerRating.Rating >= 2400)
            {
                return 20;
            }
            else
            {
                return 40;
            }
        }

        /*
        For players with more than the provisional games setting,
        we use a lower K-factor to provide more stability in their rating as they have an established skill level.
        However, if their rating is very high (e.g., 2400 or above),
        we can use an even lower K-factor to prevent excessive rating changes from a single game,
        as they are likely already at an established skill level.
        */
        else
        {
            if (playerRating.Rating >= 2400)
            {
                return 10;
            }
            else
            {
                return 15;
            }
        }
    }
}
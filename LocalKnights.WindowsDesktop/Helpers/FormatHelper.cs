using LocalKnights.Common.Helpers;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.DisplayObjects;

namespace LocalKnights.WindowsDesktop.Helpers
{
    /// <summary>
    /// Provides helper methods for formatting chess-related data for display in views.
    /// </summary>
    public static class FormatHelper
    {
        /// <summary>
        /// Formats a player's rating for display based on the specified time format and club preferences. It retrieves the player's rating for the given time format and applies the appropriate formatting rules based on the club's rating system (Glicko-2 or ELO). For Glicko-2, it checks if the deviation is above the uncertain threshold to determine if a "?" should be appended. For ELO, it checks if the player has played less than the provisional games setting within last year to determine if a "?" should be appended. If the rating system is not recognized, it falls back to formatting just the rating value without any uncertainty indication.
        /// </summary>
        /// <param name="player">The player for whom to format the rating.</param>
        /// <param name="timeFormat">The time format for which to retrieve the rating.</param>
        /// <param name="clubPreferences">The club's rating preferences.</param>
        /// <param name="playerService">The player service for retrieving player data.</param>
        /// <param name="supportEloProvisoryDisplay">Whether to support the display of provisional ELO ratings based on games played since last year (requires additional database queries and computation).</param>
        /// <returns>The formatted rating string.</returns>
        public static async Task<string> FormatRating(Player player,
            TimeFormat timeFormat,
            ClubPreferences clubPreferences,
            IPlayerService playerService,
            bool supportEloProvisoryDisplay = true)
        {
            int? ratedGamesPlayedSinceLastYear = null;

            if (clubPreferences.RatingSystem == RatingSystem.ELO && supportEloProvisoryDisplay)
            {
                ratedGamesPlayedSinceLastYear = await playerService.GetPlayerRatedGamesCountSinceLastYear(player.IdentifierName, timeFormat);
            }

            return FormatRating(player.GetRatingForTimeFormat(timeFormat), clubPreferences.RatingSystem, ratedGamesPlayedSinceLastYear, clubPreferences.ProvisionalGames);
        }

        /// <summary>
        /// Formats a chess rating for display based on the specified rating system.
        /// For Glicko-2, if the deviation is above the uncertain threshold, it appends a "?" to indicate uncertainty.
        /// For ELO, if the player has played less than the provisional games setting within last year,
        /// it appends a "?" to indicate that the rating is provisional and may be less accurate.
        /// If the rating system is not recognized, it falls back to formatting just the rating value without any uncertainty indication.
        /// </summary>
        /// <param name="rating">A chess rating</param>
        /// <param name="ratingSystem">The rating system</param>
        /// <param name="ratedGamesPlayedSinceLastYear">The number of rated games the player has played since last year</param>
        /// <param name="provisionalGamesSetting">The number of games a player is considered provisional</param>
        /// <returns>Formatted rating string</returns>
        public static string FormatRating(ChessRating rating, RatingSystem ratingSystem, int? ratedGamesPlayedSinceLastYear = null, int? provisionalGamesSetting = null)
        {
            return ratingSystem switch
            {
                RatingSystem.Glicko2 => FormatRatingGlicko2(rating),
                RatingSystem.ELO when ratedGamesPlayedSinceLastYear.HasValue && provisionalGamesSetting.HasValue =>
                    FormatRatingElo(rating, ratedGamesPlayedSinceLastYear.Value, provisionalGamesSetting.Value),
                _ => $"{rating.Rating:F0}"
            };
        }

        /// <summary>
        /// Formats a rating for display. If the deviation is above the uncertain threshold, it appends a "?" to indicate uncertainty.
        /// </summary>
        /// <param name="rating">A chess rating</param>
        /// <returns>Formatted rating string</returns>
        public static string FormatRatingGlicko2(ChessRating rating)
        {
            return rating.Deviation > CommonConstants.UncertainDeviationThreshold
                ? $"{rating.Rating:F0}?"
                : $"{rating.Rating:F0}";
        }

        /// <summary>
        /// Formats a rating for display. If the player has played less than the provisional games setting within last year, it appends a "?" to indicate that the rating is provisional and may be less accurate.
        /// </summary>
        /// <param name="rating">A chess rating</param>
        /// <param name="ratedGamesPlayedSinceLastYear">The number of rated games the player has played since last year</param>
        /// <param name="provisionalGamesSetting">The number of games a player is considered provisional</param>
        /// <returns>Formatted rating string</returns>
        public static string FormatRatingElo(ChessRating rating, int ratedGamesPlayedSinceLastYear, int provisionalGamesSetting)
        {
            return ratedGamesPlayedSinceLastYear < provisionalGamesSetting
                ? $"{rating.Rating:F0}?"
                : $"{rating.Rating:F0}";
        }

        /// <summary>
        /// Formats the rating change for both players in a game. Positive changes are prefixed with "+", while negative changes are shown as-is.
        /// </summary>
        /// <param name="whiteChange">The rating change for the white player</param>
        /// <param name="blackChange">The rating change for the black player</param>
        /// <returns>Formatted rating change string</returns>
        public static string FormatRatingChange(int whiteChange, int blackChange)
        {
            string FormatChange(int change) => change switch
            {
                >= 0 => $"+{change}",
                < 0 => $"{change}"
            };

            return $"{FormatChange(whiteChange)} \t {FormatChange(blackChange)}";
        }

        /// <summary>
        /// Formats a chess rating along with the player's position on the leaderboard for that time format. The position is displayed in parentheses after the rating.
        /// </summary>
        /// <param name="formattedRating">A chess rating already formatted as string</param>
        /// <param name="playerPosition">The player's position on the leaderboard</param>
        /// <returns>Formatted rating string with position</returns>
        public static string FormatRatingWithPosition(string formattedRating, int playerPosition)
        {
            return  $"{formattedRating} (#{playerPosition})";
        }

        /// <summary>
        /// Formats the participants of a game for display. It shows the white and black player's names along with their ratings in parentheses. If the player has been deleted, use the player identifier name that has been recorded onto the game.
        /// </summary>
        /// <param name="game">The game for which to format participants</param>
        /// <param name="whitePlayer">The white player (can be null if the player has been deleted)</param>
        /// <param name="blackPlayer">The black player (can be null if the player has been deleted)</param>
        /// <returns>Formatted string representing the game participants</returns>
        public static string FormatGameParticipants(Game game, Player? whitePlayer, Player? blackPlayer)
        {
            string whiteName = whitePlayer == null ? game.WhitePlayerId : whitePlayer.DisplayName;
            string blackName = blackPlayer == null ? game.BlackPlayerId : blackPlayer.DisplayName;

            string whiteRating = game.WhitePlayerRating != null ? FormatRatingGlicko2(game.WhitePlayerRating) : string.Empty;
            string blackRating = game.BlackPlayerRating != null ? FormatRatingGlicko2(game.BlackPlayerRating) : string.Empty;

            string whiteText = string.IsNullOrEmpty(whiteRating) ? whiteName : $"{whiteName} ({whiteRating})";
            string blackText = string.IsNullOrEmpty(blackRating) ? blackName : $"{blackName} ({blackRating})";

            return $"{whiteText} x {blackText}";
        }

        /// <summary>
        /// Parses a game result string into a GameResult enum value.
        /// </summary>
        /// <param name="value">The game result string</param>
        /// <returns>The parsed game result</returns>
        public static GameResult? ParseGameResultString(string value)
        {
            return value switch
            {
                CommonConstants.WhiteWinResultString => GameResult.WhiteWins,
                CommonConstants.BlackWinResultString => GameResult.BlackWins,
                CommonConstants.DrawResultString => GameResult.Draw,
                "1/2-1/2" => GameResult.Draw,
                "1 - 0" => GameResult.WhiteWins,
                "0 - 1" => GameResult.BlackWins,
                "1/2 - 1/2" => GameResult.Draw,
                _ => null,
            };
        }

        /// <summary>
        /// Shows the participant displayname with rating on the side.
        /// </summary>
        /// <param name="clubPreferences">The club preferences</param>
        /// <param name="participant">The competition participant</param>
        /// <param name="player">The player object</param>
        public static string FormatCompetitionParticipantNameWithRating(ClubPreferences clubPreferences, CompetitionParticipant participant, Player player)
        {
            ArgumentNullException.ThrowIfNull(participant, nameof(participant));
            ArgumentNullException.ThrowIfNull(clubPreferences, nameof(clubPreferences));

            return $"{(player != null ? player.DisplayName : participant.ParticipantIdentifierName)} ({FormatHelper.FormatRating(participant.OldRating, clubPreferences.RatingSystem)})";
        }

        /// <summary>
        /// Convert the Participant entry data object into a presentation object for the participants table in Competition page.
        /// </summary>
        /// <param name="clubPreferences">The club preferences</param>
        /// <param name="participant">The competition participant</param>
        /// <returns>The parsed game result</returns>
        public static CompetitionPageParticipantTableItem FormatCompetitionPageParticipantTableItem(ClubPreferences clubPreferences, CompetitionParticipant participant, Player player)
        {
            ArgumentNullException.ThrowIfNull(participant, nameof(participant));
            ArgumentNullException.ThrowIfNull(clubPreferences, nameof(clubPreferences));

            return new CompetitionPageParticipantTableItem()
            {
                DisplayName = player == null ? participant.ParticipantIdentifierName : player.DisplayName,
                IdentifierName = participant.ParticipantIdentifierName,
                RatingDisplay = FormatRating(participant.OldRating, clubPreferences.RatingSystem),
                CanNavigateToPlayer = player != null,
            };
        }

        /// <summary>
        /// Convert the Standing entry data object into a presentation object for the standings table in Competition page.
        /// </summary>
        /// <param name="clubPreferences">The club preferences</param>
        /// <param name="standingEntry">The standing entry data object</param>
        /// <param name="player">The player object</param>
        /// <returns>The parsed game result</returns>
        public static CompetitionPageStandingTableItem FormatCompetitionPageStandingTableItem(ClubPreferences clubPreferences, CompetitionStandingEntry standingEntry, Player player)
        {
            ArgumentNullException.ThrowIfNull(standingEntry, nameof(standingEntry));

            return new CompetitionPageStandingTableItem()
            {
                Classification = $"#{standingEntry.Classification}",
                ParticipantName = player == null ? standingEntry.Participant.ParticipantIdentifierName : $"{player.DisplayName} ({player.IdentifierName})",
                Score = standingEntry.Score.ToString("F1"),
                OldRatingDisplay = FormatRating(standingEntry.Participant.OldRating, clubPreferences.RatingSystem, null, null),
                NewRatingDisplay = standingEntry.Participant.NewRating == null ? string.Empty : FormatRating(standingEntry.Participant.NewRating, clubPreferences.RatingSystem, null, null),
                PerformanceRatingDisplay = standingEntry.PerformanceRating?.ToString("F0") ?? string.Empty,
            };
        }
    }
}
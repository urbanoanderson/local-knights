using System;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers
{
    /// <summary>
    /// Provides application-wide constant values.
    /// </summary>
    public static class CommonConstants
    {
        /// <summary>
        /// The application name to be displayed.
        /// </summary>
        public const string AppDisplayName = "Local Knights";

        public const int IdentifierMaxLength = 50;
        public const int DisplayNameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int CityMaxLength = 100;
        public const int ChiefArbiterMaxLength = 100;
        public const int FederationCodeLength = 3;
        public const int SocialMediaLinkLimit = 5;
        public const int ExternalUsernameMaxLength = 50;
        public const int FederationIdMaxLength = 12;
        public const int EmailMaxLength = 100;
        public const int PhoneMaxLength = 20;
        public const int CompetitionTitleMaxLength = 100;
        public const int CompetitionRoundsMinimum = 1;
        public const int CompetitionRoundsMaximum = 30;
        public const double CompetitionRoundsMinimumControlValue = CompetitionRoundsMinimum;
        public const double CompetitionRoundsMaximumControlValue = CompetitionRoundsMaximum;

        /// <summary>
        /// The default rating for new players.
        /// </summary>
        public const double DefaultRating = 1500;

        /// <summary>
        /// The default rating deviation for new players.
        /// </summary>
        public const double DefaultDeviation = 350;

        /// <summary>
        /// Lower bound for default deviation values configured by the club.
        /// </summary>
        public const double DefaultDeviationLowerBound = 150;

        /// <summary>
        /// Upper bound for default deviation values configured by the club.
        /// </summary>
        public const double DefaultDeviationUpperBound = 1000;

        /// <summary>
        /// The default rating volatility for new players.
        /// </summary>
        public const double DefaultVolatility = 0.06;

        /// <summary>
        /// Lower bound for default volatility values configured by the club.
        /// </summary>
        public const double DefaultVolatilityLowerBound = 0.01;

        /// <summary>
        /// Upper bound for default volatility values configured by the club.
        /// </summary>
        public const double DefaultVolatilityUpperBound = 0.1;

        /// <summary>
        /// The default provisional games count for new players in the ELO system.
        /// </summary>
        public const int DefaultProvisionalGames = 20;

        /// <summary>
        /// Lower bound for default provisional games configured by the club.
        /// </summary>
        public const int DefaultProvisionalGamesLowerBound = 0;

        /// <summary>
        /// Upper bound for default provisional games configured by the club.
        /// </summary>
        public const int DefaultProvisionalGamesUpperBound = 100;

        /// <summary>
        /// The threshold for uncertain rating deviation.
        /// </summary>
        public const double UncertainDeviationThreshold = 110;

        /// <summary>
        /// The minimum rating allowed for a player. Useful to prevent ratings from going too low and becoming meaningless.
        /// </summary>
        public const double DefaultMinRating = 100;

        /// <summary>
        /// Lower bound for the club minimum rating clamp.
        /// </summary>
        public const double DefaultMinRatingLowerBound = 100;

        /// <summary>
        /// Upper bound for the club minimum rating clamp.
        /// </summary>
        public const double DefaultMinRatingUpperBound = 500;

        /// <summary>
        /// The maximum rating allowed for a player. Useful to prevent ratings from going too high and becoming meaningless.
        /// </summary>
        public const double DefaultMaxRating = 4000;

        /// <summary>
        /// Lower bound for the club maximum rating clamp.
        /// </summary>
        public const double DefaultMaxRatingLowerBound = 3000;

        /// <summary>
        /// Upper bound for the club maximum rating clamp.
        /// </summary>
        public const double DefaultMaxRatingUpperBound = 4000;

        /// <summary>
        /// The default win value for a player who wins a game. This is typically a full point, but can be adjusted based on the tournament rules.
        ///  </summary>
        public const double DefaultWinScoreValue = 1.0;

        /// <summary>
        /// The default loss value for a player who loses a game. This is typically zero points, but can be adjusted based on the tournament rules.
        /// </summary>
        public const double DefaultLossScoreValue = 0.0;

        /// <summary>
        /// The default draw value for a player who draws a game. This is typically half a point, but can be adjusted based on the tournament rules.
        /// </summary>
        public const double DefaultDrawScoreValue = 0.5;

        /// <summary>
        /// The default number of rounds for a competition. This is typically 5 rounds, but can be adjusted based on the tournament rules.
        /// </summary>
        public const int DefaultCompetitionNumberOfRounds = 5;

        /// <summary>
        /// The default value for whether a competition is rated or not. This is typically true, meaning competitions are rated by default, but can be adjusted based on the tournament rules.
        /// </summary>
        public const bool DefaultCompetitionIsRatedValue = true;

        /// <summary>
        /// The default bye value for a player who receives a bye in a round. This is typically half a point, but can be adjusted based on the tournament rules.
        ///  </summary>
        public const double DefaultByeValue = DefaultDrawScoreValue;

        /// <summary>
        /// The default TimeFormat value selected on creation forms.
        ///  </summary>
        public const TimeFormat DefaultTimeFormat = TimeFormat.Blitz;

        /// <summary>
        /// The string representing a win result for white.
        /// </summary>
        public const string WhiteWinResultString = "1-0";

        /// <summary>
        /// The string representing a win result for black.
        /// </summary>
        public const string BlackWinResultString = "0-1";

        /// <summary>
        /// The string representing a draw result.
        /// </summary>
        public const string DrawResultString = "½-½";
    }
}

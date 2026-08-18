using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocalKnights.Common.Helpers;

namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents the preferences and settings for a chess club stored in a single row.
    /// Each database file contains exactly one ClubPreferences row representing the club data.
    /// </summary>
    public class ClubPreferences
    {
        /// <summary>
        /// Gets or sets the unique identifier for this row.
        /// </summary>
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>
        /// Gets or sets the display name of the club.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the club.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the collection of social media links associated with the club.
        /// </summary>
        public List<string>? SocialMediaLinks { get; set; }

        /// <summary>
        /// Gets or sets the default rating for new players in the club.
        /// </summary>
        public double Rating { get; set; } = CommonConstants.DefaultRating;

        /// <summary>
        /// Gets or sets the minimum rating allowed for players in the club.
        /// </summary>
        public double MinRating { get; set; } = CommonConstants.DefaultMinRating;

        /// <summary>
        /// Gets or sets the maximum rating allowed for players in the club.
        /// </summary>
        public double MaxRating { get; set; } = CommonConstants.DefaultMaxRating;

        /// <summary>
        /// Gets or sets the default deviation for new players ratings in the club (Glicko-2 parameter).
        /// Represents the standard deviation of the player's rating.
        /// </summary>
        public double Deviation { get; set; } = CommonConstants.DefaultDeviation;

        /// <summary>
        /// Gets or sets the volatility for new players in the club (Glicko-2 parameter).
        /// Measures how much the rating is expected to change. Default is 0.06. Valid range is 0.01-0.1.
        /// </summary>
        [Range(0.01, 0.1)]
        public double Volatility { get; set; } = CommonConstants.DefaultVolatility;

        /// <summary>
        /// Gets or sets the rating system used by the club.
        /// Default is Glicko-2.
        /// </summary>
        public RatingSystem RatingSystem { get; set; } = RatingSystem.Glicko2;

        /// <summary>
        /// Gets or sets the number of games a player is considered provisional in the ELO system.
        /// Default is 20. Valid range is 0-100.
        /// </summary>
        [Range(0, 100)]
        public int ProvisionalGames { get; set; } = CommonConstants.DefaultProvisionalGames;

        /// <summary>
        /// Schema version for the ClubPreferences row. Reserved for future schema migration
        /// logic. Default is 0.
        /// </summary>
        public int SchemaVersion { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date and time when the club was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the club preferences were last updated (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Unique club identifier which is used to name the database file and is not stored in the database itself.
        /// </summary>
        [NotMapped]
        public string IdentifierName { get; set; } = string.Empty;

        /// <summary>
        /// Helper method to get a default club rating object.
        /// </summary>
        public ChessRating GetDefaultClubRating()
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
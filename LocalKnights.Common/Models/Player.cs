using System.ComponentModel.DataAnnotations;

namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents a player in the chess club with ratings for different time formats.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Gets or sets the unique identifier name for the player (lowercase alphanumeric, primary key).
        /// </summary>
        [Key]
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-z0-9]+$")]
        public string IdentifierName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the player.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// UNUSED FOR NOW - Optional Notes on the Player.
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; } = null;

        /// <summary>
        /// UNUSED FOR NOW - Optional Address of the Player.
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; } = null;

        /// <summary>
        /// Gets or sets the Lichess username associated with the player.
        /// </summary>
        [StringLength(50)]
        public string? LichessUsername { get; set; }

        /// <summary>
        /// Gets or sets the Chess.com username associated with the player.
        /// </summary>
        [StringLength(50)]
        public string? ChessComUsername { get; set; }

        /// <summary>
        /// Gets or sets the email address for the player.
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number for the player.
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the International Chess Federation (FIDE) identifier for the player.
        /// </summary>
        [StringLength(12)]
        [RegularExpression(@"^\d{1,12}$")]
        public string? FideId { get; set; }

        /// <summary>
        /// Gets or sets the Brazilian Chess Confederation (CBX) identifier for the player.
        /// </summary>
        [StringLength(12)]
        [RegularExpression(@"^\d{1,12}$")]
        public string? CbxId { get; set; }

        /// <summary>
        /// Gets or sets the United States Chess Federation (USCF) identifier for the player.
        /// </summary>
        [StringLength(12)]
        [RegularExpression(@"^\d{1,12}$")]
        public string? UscfId { get; set; }

        /// <summary>
        /// Gets or sets the chess title of the player (e.g., GM, IM, FM, etc.).
        /// </summary>
        public ChessTitle ChessTitle { get; set; } = ChessTitle.None;

        /// <summary>
        /// Gets or sets the player's gender.
        /// </summary>
        public GenderType Gender { get; set; } = GenderType.Other;

        /// <summary>
        /// Gets or sets the date and time when the player was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the player's birth date.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the chess rating for bullet time format (1 minute to 2 minutes).
        /// </summary>
        public ChessRating BulletRating { get; set; } = new();

        /// <summary>
        /// Gets or sets the chess rating for blitz time format (3 minutes to 8 minutes).
        /// </summary>
        public ChessRating BlitzRating { get; set; } = new();

        /// <summary>
        /// Gets or sets the chess rating for rapid time format (10 minutes to 60 minutes).
        /// </summary>
        public ChessRating RapidRating { get; set; } = new();

        /// <summary>
        /// Gets or sets the chess rating for classical time format (more than 60 minutes).
        /// </summary>
        public ChessRating ClassicalRating { get; set; } = new();

        /// <summary>
        /// Gets the chess rating for a specified time format.
        /// </summary>
        /// <param name="timeFormat">The time format for which to get the rating.</param>
        /// <returns>The player's chess rating for the specified time format.</returns>
        public ChessRating GetRatingForTimeFormat(TimeFormat timeFormat)
        {
            return timeFormat switch
            {
                TimeFormat.Bullet => this.BulletRating,
                TimeFormat.Blitz => this.BlitzRating,
                TimeFormat.Rapid => this.RapidRating,
                TimeFormat.Classical => this.ClassicalRating,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Sets the chess rating for a player in the specified time format.
        /// </summary>
        /// <param name="timeFormat">The time format for which to set the rating.</param>
        /// <param name="rating">The new chess rating value.</param>
        public void SetRatingForTimeFormat(TimeFormat timeFormat, ChessRating rating)
        {
            switch (timeFormat)
            {
                case TimeFormat.Bullet:
                    this.BulletRating = rating;
                    break;
                case TimeFormat.Blitz:
                    this.BlitzRating = rating;
                    break;
                case TimeFormat.Rapid:
                    this.RapidRating = rating;
                    break;
                case TimeFormat.Classical:
                    this.ClassicalRating = rating;
                    break;
            }
        }
    }
}
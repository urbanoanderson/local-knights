using System.ComponentModel.DataAnnotations;

namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents a chess game with players, result, and rating changes.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Gets or sets the unique identifier for the game.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the game was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time format for this game (Bullet, Blitz, Rapid, or Classical).
        /// </summary>
        public TimeFormat TimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the result of the game (WhiteWins, BlackWins, or Draw).
        /// </summary>
        public GameResult Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this game is rated (affects player ratings).
        /// </summary>
        public bool IsRated { get; set; }

        /// <summary>
        /// UNUSED FOR NOW - Optional Notes on the Game.
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; } = null;

        /// <summary>
        /// Gets or sets the Portable Game Notation (PGN) containing the game moves and metadata.
        /// </summary>
        public string? Pgn { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the white player.
        /// </summary>
        public string WhitePlayerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the black player.
        /// </summary>
        public string BlackPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the chess rating of the white player at the time the game was created.
        /// </summary>
        public ChessRating WhitePlayerRating { get; set; } = new();

        /// <summary>
        /// Gets or sets the chess rating of the black player at the time the game was created.
        /// </summary>
        public ChessRating BlackPlayerRating { get; set; } = new();

        /// <summary>
        /// Gets or sets the rating change for the white player (positive for wins/draws against higher rated, negative for losses).
        /// </summary>
        public int WhitePlayerRatingChange { get; set; }

        /// <summary>
        /// Gets or sets the rating change for the black player (positive for wins/draws against higher rated, negative for losses).
        /// </summary>
        public int BlackPlayerRatingChange { get; set; }
    }
}
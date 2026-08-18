namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents a pairing (match) within a competition round.
    /// </summary>
    public class CompetitionPairing
    {
        /// <summary>
        /// Competition identifier foreign key.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// The competition this pairing belongs to.
        /// </summary>
        public Competition? Competition { get; set; }

        /// <summary>
        /// The round number this pairing belongs to.
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// The physical table number assigned to this pairing for the round.
        /// </summary>
        public int TableNumber { get; set; }

        /// <summary>
        /// Identifier name of the white player.
        /// </summary>
        public string WhitePlayerIdentifierName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier name of the black player.
        /// </summary>
        public string BlackPlayerIdentifierName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of the game (WhiteWins, BlackWins, or Draw).
        /// </summary>
        public GameResult? Result { get; set; } = null;

        /// <summary>
        /// Id Reference to the game record for this pairing.
        /// Nullable for BYE or non-played pairings.
        /// </summary>
        public int? GameId { get; set; }

        public bool IsByePairing()
        {
            return this.TableNumber < 0 || string.IsNullOrWhiteSpace(this.BlackPlayerIdentifierName);
        }
    }
}

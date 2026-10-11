namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Join entity representing a participant in a competition (many-to-many between Competition and Player).
    /// </summary>
    public class CompetitionParticipant
    {
        public int CompetitionId { get; set; }

        public Competition? Competition { get; set; }

        public required string ParticipantIdentifierName { get; set; }

        public required ChessRating OldRating { get; set; }

        public ChessRating? NewRating { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;

namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Represents a chess competition organized by a club.
    /// </summary>
    public class Competition
    {
        /// <summary>
        /// Gets or sets the unique identifier for the competition.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the competition was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the competition was started (UTC).
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the competition was completed (UTC).
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the current state of the competition.
        /// </summary>
        public CompetitionState State { get; set; } = CompetitionState.Starting;

        /// <summary>
        /// Gets or sets the title of the competition.
        /// </summary>
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the competition.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of competition (Swiss, RoundRobin, Knockout, or Match).
        /// </summary>
        public CompetitionType Type { get; set; }

        /// <summary>
        /// Gets or sets the time format for games in this competition.
        /// </summary>
        public required TimeFormat TimeFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this competition is rated (affects player ratings).
        /// </summary>
        public bool IsRated { get; set; }

        /// <summary>
        /// Gets or sets the collection of competition participant join entities.
        /// </summary>
        public ICollection<CompetitionParticipant> CompetitionParticipants { get; set; } = new List<CompetitionParticipant>();

        /// <summary>
        /// Gets or sets the collection of competition round pairings.
        /// </summary>
        public ICollection<CompetitionPairing> CompetitionPairings { get; set; } = new List<CompetitionPairing>();

        /// <summary>
        /// Current round number of the competition (1-based). 0 if not started.
        /// </summary>
        public int CurrentRound { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of rounds (set directly by Swiss and Match competitions. Automatically calculated for other types).
        /// </summary>
        public int NumberOfRounds { get; set; }

        /// <summary>
        /// Gets or sets the bye value used in Swiss system competitions.
        /// </summary>
        public double? ByeValue { get; set; }

        /// <summary>
        /// UNUSED FOR NOW - Gets or sets how participant pairings are initially seeded when the competition starts.
        /// Default is Rating-based seeding.
        /// </summary>
        public CompetitionInitialSeeding InitialSeeding { get; set; } = CompetitionInitialSeeding.Rating;

        /// <summary>
        /// Gets or sets a value indicating whether this is a double round-robin competition (Round-Robin specific).
        /// </summary>
        public bool? IsDoubleRoundRobin { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the winner of the competition.
        /// Makes it easier to query the winner without having to calculate it from the rounds and games.
        /// </summary>
        public string? WinnerId { get; set; }

        /// <summary>
        /// Computed standings for competition based on pairings completed so far.
        /// Not persisted into database. This should be computed when loading the competition
        /// or after performing changes in the competition pairings
        /// </summary>
        [NotMapped]
        public IEnumerable<CompetitionStandingEntry> ComputedStandings { get; set; } = null;

        /// <summary>
        /// Helper method to get pairings for a round
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CompetitionPairing> GetRoundPairings(int round, bool includeByes = true)
        {
            return this.CompetitionPairings.Where(p => p.RoundNumber == round && (includeByes == true || !p.IsByePairing()));
        }

        /// <summary>
        /// Helper method to get BYE participants for a round
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CompetitionParticipant> GetRoundByeParticipants(int round)
        {
            IEnumerable<string> byeParticipantIds = this.GetRoundPairings(round)
                .Where(p => p.IsByePairing() == true)
                .Select(p => p.WhitePlayerIdentifierName);

            return this.CompetitionParticipants.Where(p => byeParticipantIds.Contains(p.ParticipantIdentifierName));
        }
    }
}
namespace LocalKnights.WindowsDesktop.DisplayObjects
{
    public class CompetitionPageStandingTableItem
    {
        public required string Classification { get; set; }

        public required string ParticipantName { get; set; }

        public required string Score { get; set; }

        public required string OldRatingDisplay { get; set; }

        public required string NewRatingDisplay { get; set; } = string.Empty;

        public required string PerformanceRatingDisplay { get; set; } = string.Empty;

        public required string BuchholzScoreDisplay { get; set; } = string.Empty;

        public required string SonnebornBergerScoreDisplay { get; set; } = string.Empty;
    }
}
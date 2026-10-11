
namespace LocalKnights.WindowsDesktop.DisplayObjects
{
    public sealed class CompetitionPageParticipantTableItem
    {
        public required string DisplayName { get; set; }

        public required string IdentifierName { get; set; }

        public required string RatingDisplay { get; set; }

        public required bool CanNavigateToPlayer { get; set; }
    }
}


using LocalKnights.Common.Models;

namespace LocalKnights.WindowsDesktop.DisplayObjects
{
    public class PlayersPageTableItem
    {
        public string IdentifierName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public ChessTitle ChessTitle { get; set; } = ChessTitle.None;
        public DateTime? BirthDate { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string BulletRating { get; set; } = string.Empty;
        public string BlitzRating { get; set; } = string.Empty;
        public string RapidRating { get; set; } = string.Empty;
        public string ClassicalRating { get; set; } = string.Empty;
    }
}
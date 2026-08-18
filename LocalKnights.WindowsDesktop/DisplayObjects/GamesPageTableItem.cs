namespace LocalKnights.WindowsDesktop.DisplayObjects
{
    public class GamesPageTableItem
    {
        public int Id { get; set; }
        public DateTime CreatedAtSortKey { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string Participants { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string TimeFormat { get; set; } = string.Empty;
        public string Rated { get; set; } = string.Empty;
        public string RatingChange { get; set; } = string.Empty;
        public string CompetitionId { get; set; } = string.Empty;
    }
}
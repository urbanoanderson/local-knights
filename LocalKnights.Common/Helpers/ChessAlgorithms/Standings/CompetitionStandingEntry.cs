using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Standings
{
    public class CompetitionStandingEntry
    {
        public required CompetitionParticipant Participant { get; set; }

        public double Score { get; set; }

        public double BuchholzScore { get; set; }

        public double SonnebornBergerScore { get; set; }

        public double? PerformanceRating { get; set; }

        // Classification (1 = champion, ...)
        public int Classification { get; set; }
    }
}
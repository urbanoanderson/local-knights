using LocalKnights.Common.Models;
using LocalKnights.Common.Services;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Pairing
{
    /// <summary>
    /// Helper class for generating pairings in competitions.
    /// </summary>
    public static class PairingHelper
    {
        /// <summary>
        /// Generates pairings for the current round in a competition.
        /// </summary>
        /// <param name="competition">The competition.</param>
        /// <param name="randomService">The random service used by algorithms that require randomness.</param>
        /// <returns>A collection of pairings.</returns>
        public static IEnumerable<CompetitionPairing> GenerateCurrentRoundPairings(Competition competition, IRandomService randomService)
        {
            IEnumerable<CompetitionPairing> roundPairings = competition.Type switch
            {
                CompetitionType.ChessMatch => MatchPairingAlgorithm.GenerateCurrentRoundPairings(competition, randomService),
                CompetitionType.SwissSystemTournament => SwissPairingAlgorithm.GenerateCurrentRoundPairings(competition),
                CompetitionType.RoundRobinTournament => RoundRobinPairingAlgorithm.GenerateCurrentRoundPairings(competition),
                CompetitionType.KnockoutTournament => KnockoutPairingAlgorithm.GenerateCurrentRoundPairings(competition, randomService),
                _ => new List<CompetitionPairing>()
            };

            // Make sure generated parirings are related to the competition and round
            foreach (CompetitionPairing p in roundPairings)
            {
                p.CompetitionId = competition.Id;
                p.Competition = null;
                p.RoundNumber = competition.CurrentRound;
            }

            return roundPairings;
        }
    }
}

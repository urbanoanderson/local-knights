using LocalKnights.Common.Models;
using LocalKnights.Common.Services;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Pairing
{
    /// <summary>
    /// Generates pairings for chess match competitions.
    /// </summary>
    public static class MatchPairingAlgorithm
    {
        /// <summary>
        /// Generates a single pairing for a chess match competition.
        /// </summary>
        /// <param name="competition">The competition.</param>
        /// <param name="randomService">The random service.</param>
        /// <returns>The generated pairings.</returns>
        public static IEnumerable<CompetitionPairing> GenerateCurrentRoundPairings(Competition competition, IRandomService randomService)
        {
            if (competition.CompetitionParticipants.Count != 2)
            {
                return new List<CompetitionPairing>();
            }

            List<CompetitionParticipant> participants = competition.CompetitionParticipants.ToList();

            bool whiteIsFirstPlayer;

            if (competition.CurrentRound > 1 && competition.CompetitionPairings != null)
            {
                CompetitionPairing? previousPairing = competition.CompetitionPairings
                    .FirstOrDefault(cp => cp.RoundNumber == competition.CurrentRound - 1 &&
                        NormalizePair(cp.WhitePlayerIdentifierName, cp.BlackPlayerIdentifierName) == NormalizePair(participants[0].ParticipantIdentifierName, participants[1].ParticipantIdentifierName));

                if (previousPairing != null)
                {
                    whiteIsFirstPlayer = !string.Equals(previousPairing.WhitePlayerIdentifierName, participants[0].ParticipantIdentifierName, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    whiteIsFirstPlayer = randomService.NextBool();
                }
            }
            else
            {
                whiteIsFirstPlayer = randomService.NextBool();
            }

            return new List<CompetitionPairing>
            {
                new CompetitionPairing
                {
                    CompetitionId = competition.Id,
                    Competition = competition,
                    RoundNumber = competition.CurrentRound,
                    WhitePlayerIdentifierName = whiteIsFirstPlayer ? participants[0].ParticipantIdentifierName : participants[1].ParticipantIdentifierName,
                    BlackPlayerIdentifierName = whiteIsFirstPlayer ? participants[1].ParticipantIdentifierName : participants[0].ParticipantIdentifierName,
                    TableNumber = 1,
                }
            };
        }

        private static (string White, string Black) NormalizePair(string firstPlayerId, string secondPlayerId)
        {
            return string.Compare(firstPlayerId, secondPlayerId, StringComparison.OrdinalIgnoreCase) <= 0
                ? (firstPlayerId, secondPlayerId)
                : (secondPlayerId, firstPlayerId);
        }
    }
}

using LocalKnights.Common.Models;
using LocalKnights.Common.Services;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Pairing
{
    /// <summary>
    /// Generates pairings for knockout competitions.
    /// </summary>
    public static class KnockoutPairingAlgorithm
    {
        private sealed class ParticipantSeed
        {
            public required string IdentifierName { get; init; }

            public required ChessRating Rating { get; init; }
        }

        /// <summary>
        /// Calculates the number of rounds needed for a knockout tournament based on the number of participants.
        /// </summary>
        /// <param name="participantCount">The number of participants.</param>
        /// <returns>The calculated number of rounds.</returns>
        public static int CalculateNumberOfRounds(int participantCount)
        {
            return (int)Math.Ceiling(Math.Log2(participantCount));
        }

        /// <summary>
        /// Generates knockout pairings for the specified round.
        /// </summary>
        /// <param name="competition">The competition.</param>
        /// <param name="randomService">The random service.</param>
        /// <returns>The generated pairings.</returns>
        public static IEnumerable<CompetitionPairing> GenerateCurrentRoundPairings(Competition competition, IRandomService randomService)
        {
            List<ParticipantSeed> seeds = competition.CompetitionParticipants
                .Select(participant => new ParticipantSeed
                {
                    IdentifierName = participant.ParticipantIdentifierName,
                    Rating = participant.OldRating
                })
                .OrderByDescending(participant => participant.Rating.Rating)
                .ThenBy(participant => participant.IdentifierName)
                .ToList();

            List<CompetitionPairing> pairings = new();

            if (seeds.Count < 1)
            {
                return pairings;
            }

            int n = seeds.Count;
            Dictionary<string, int> seedIndex = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < n; i++)
            {
                seedIndex[seeds[i].IdentifierName] = i;
            }

            if (competition.CurrentRound == 1)
            {
                AddRoundPairingsFromActivePlayers(
                    activePlayers: seeds,
                    competition: null,
                    roundNumber: competition.CurrentRound,
                    pairings,
                    randomService,
                    seedIndex);

                AddPairingTableNumbers(pairings);
                return pairings;
            }

            List<ParticipantSeed> activePlayers = DetermineActivePlayers(competition, seeds, seedIndex);

            if (activePlayers.Count < 2 && !ShouldGenerateFinalPlacementPairing(competition, activePlayers.Count))
            {
                return pairings;
            }

            if (ShouldGenerateFinalPlacementPairing(competition, activePlayers.Count))
            {
                AddFinalRoundPairings(competition, pairings, randomService);
            }
            else
            {
                AddRoundPairingsFromActivePlayers(
                    activePlayers,
                    competition,
                    competition.CurrentRound,
                    pairings,
                    randomService,
                    seedIndex);
            }

            AddPairingTableNumbers(pairings);

            return pairings;
        }

        private static void AddPairingTableNumbers(List<CompetitionPairing> pairings)
        {
            int byeCount = 0;
            int tableNumber = 0;

            foreach (CompetitionPairing pairing in pairings)
            {
                if (!string.IsNullOrWhiteSpace(pairing.BlackPlayerIdentifierName))
                {
                    tableNumber++;
                    pairing.TableNumber = tableNumber;
                }
                else
                {
                    byeCount++;
                    pairing.TableNumber = -byeCount;
                }
            }
        }

        private static List<ParticipantSeed> DetermineActivePlayers(
            Competition competition,
            List<ParticipantSeed> seeds,
            IReadOnlyDictionary<string, int> seedIndex)
        {
            List<ParticipantSeed> activePlayers = seeds
                .OrderBy(player => seedIndex[player.IdentifierName])
                .ToList();

            for (int roundNumber = 1; roundNumber < competition.CurrentRound; roundNumber++)
            {
                int byeCount = GetRoundByeCount(activePlayers.Count);
                List<ParticipantSeed> nextRoundPlayers = activePlayers.Take(byeCount).ToList();
                List<ParticipantSeed> pairedPlayers = activePlayers.Skip(byeCount).ToList();

                for (int i = 0; i < pairedPlayers.Count / 2; i++)
                {
                    ParticipantSeed highSeed = pairedPlayers[i];
                    ParticipantSeed lowSeed = pairedPlayers[pairedPlayers.Count - 1 - i];
                    nextRoundPlayers.Add(DeterminePairWinner(competition, roundNumber, highSeed, lowSeed, seedIndex));
                }

                activePlayers = nextRoundPlayers
                    .OrderBy(player => seedIndex[player.IdentifierName])
                    .ToList();
            }

            return activePlayers;
        }

        private static void AddRoundPairingsFromActivePlayers(
            List<ParticipantSeed> activePlayers,
            Competition? competition,
            int roundNumber,
            List<CompetitionPairing> pairings,
            IRandomService randomService,
            IReadOnlyDictionary<string, int> seedIndex)
        {
            int byeCount = GetRoundByeCount(activePlayers.Count);

            foreach (ParticipantSeed byePlayer in activePlayers.Take(byeCount))
            {
                pairings.Add(new CompetitionPairing
                {
                    WhitePlayerIdentifierName = byePlayer.IdentifierName,
                    BlackPlayerIdentifierName = string.Empty
                });
            }

            List<ParticipantSeed> pairedPlayers = activePlayers.Skip(byeCount).ToList();
            for (int i = 0; i < pairedPlayers.Count / 2; i++)
            {
                ParticipantSeed highSeed = pairedPlayers[i];
                ParticipantSeed lowSeed = pairedPlayers[pairedPlayers.Count - 1 - i];
                AddOrCreatePairing(competition, roundNumber, highSeed, lowSeed, pairings, randomService, allowExistingLookup: competition != null);
            }
        }

        private static int GetRoundByeCount(int activePlayerCount)
        {
            int nextLowerPowerOfTwo = 1;
            while (nextLowerPowerOfTwo * 2 <= activePlayerCount)
            {
                nextLowerPowerOfTwo *= 2;
            }

            return Math.Max(0, activePlayerCount - nextLowerPowerOfTwo);
        }

        private static bool ShouldGenerateFinalPlacementPairing(Competition competition, int currentBracketLength)
        {
            return competition.CurrentRound == competition.NumberOfRounds
                && competition.NumberOfRounds > 1
                && currentBracketLength <= 2;
        }

        private static void AddFinalRoundPairings(
            Competition competition,
            List<CompetitionPairing> pairings,
            IRandomService randomService)
        {
            List<ParticipantSeed> semifinalWinners = GetSemifinalWinners(competition);

            ParticipantSeed? finalistOne = semifinalWinners.FirstOrDefault();
            ParticipantSeed? finalistTwo = semifinalWinners.Skip(1).FirstOrDefault();

            if (finalistOne != null && finalistTwo != null)
            {
                AddOrCreatePairing(competition, competition.CurrentRound, finalistOne, finalistTwo, pairings, randomService);
            }
            else if (finalistOne != null || finalistTwo != null)
            {
                ParticipantSeed finalist = finalistOne ?? finalistTwo!;
                pairings.Add(new CompetitionPairing { WhitePlayerIdentifierName = finalist.IdentifierName, BlackPlayerIdentifierName = string.Empty });
            }

            foreach ((ParticipantSeed first, ParticipantSeed second) semifinal in GetSemifinalLoserPairings(competition))
            {
                AddOrCreatePairing(competition, competition.CurrentRound, semifinal.first, semifinal.second, pairings, randomService);
            }
        }

        private static List<ParticipantSeed> GetSemifinalWinners(Competition competition)
        {
            int semifinalRound = competition.CurrentRound - 1;
            if (semifinalRound < 1)
            {
                return [];
            }

            Dictionary<string, ParticipantSeed> playersById = GetParticipantsById(competition);

            return competition.CompetitionPairings
                .Where(pairing => pairing.RoundNumber == semifinalRound)
                .Select(pairing => TryGetWinner(pairing, playersById))
                .Where(player => player != null)
                .Cast<ParticipantSeed>()
                .ToList();
        }

        private static ParticipantSeed DeterminePairWinner(
            Competition competition,
            int roundNumber,
            ParticipantSeed highSeed,
            ParticipantSeed lowSeed,
            IReadOnlyDictionary<string, int> seedIndex)
        {
            CompetitionPairing? recorded = competition.CompetitionPairings?
                .FirstOrDefault(cp => cp.RoundNumber == roundNumber &&
                    NormalizePair(cp.WhitePlayerIdentifierName, cp.BlackPlayerIdentifierName) == NormalizePair(highSeed.IdentifierName, lowSeed.IdentifierName));

            if (recorded?.Result != null)
            {
                string? winnerId = recorded.Result switch
                {
                    GameResult.WhiteWins => recorded.WhitePlayerIdentifierName,
                    GameResult.BlackWins => recorded.BlackPlayerIdentifierName,
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(winnerId))
                {
                    return string.Equals(winnerId, highSeed.IdentifierName, StringComparison.OrdinalIgnoreCase)
                        ? highSeed
                        : lowSeed;
                }
            }

            return seedIndex[highSeed.IdentifierName] <= seedIndex[lowSeed.IdentifierName] ? highSeed : lowSeed;
        }

        private static IEnumerable<(ParticipantSeed First, ParticipantSeed Second)> GetSemifinalLoserPairings(Competition competition)
        {
            int semifinalRound = competition.CurrentRound - 1;
            if (semifinalRound < 1)
            {
                yield break;
            }

            Dictionary<string, ParticipantSeed> playersById = GetParticipantsById(competition);

            List<ParticipantSeed> semifinalLosers = competition.CompetitionPairings
                .Where(pairing => pairing.RoundNumber == semifinalRound)
                .Select(pairing => TryGetLoser(pairing, playersById))
                .Where(player => player != null)
                .Cast<ParticipantSeed>()
                .ToList();

            if (semifinalLosers.Count == 2)
            {
                yield return (semifinalLosers[0], semifinalLosers[1]);
            }
        }

        private static ParticipantSeed? TryGetLoser(CompetitionPairing pairing, IReadOnlyDictionary<string, ParticipantSeed> playersById)
        {
            string? loserId = pairing.Result switch
            {
                GameResult.WhiteWins => pairing.BlackPlayerIdentifierName,
                GameResult.BlackWins => pairing.WhitePlayerIdentifierName,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(loserId))
            {
                return null;
            }

            return playersById.TryGetValue(loserId, out ParticipantSeed? loser) ? loser : null;
        }

        private static ParticipantSeed? TryGetWinner(CompetitionPairing pairing, IReadOnlyDictionary<string, ParticipantSeed> playersById)
        {
            string? winnerId = pairing.Result switch
            {
                GameResult.WhiteWins => pairing.WhitePlayerIdentifierName,
                GameResult.BlackWins => pairing.BlackPlayerIdentifierName,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(winnerId))
            {
                return null;
            }

            return playersById.TryGetValue(winnerId, out ParticipantSeed? winner) ? winner : null;
        }

        private static Dictionary<string, ParticipantSeed> GetParticipantsById(Competition competition)
        {
            return competition.CompetitionParticipants
                .ToDictionary(
                    participant => participant.ParticipantIdentifierName,
                    participant => new ParticipantSeed
                    {
                        IdentifierName = participant.ParticipantIdentifierName,
                        Rating = participant.OldRating
                    },
                    StringComparer.OrdinalIgnoreCase);
        }

        private static void AddOrCreatePairing(
            Competition? competition,
            int roundNumber,
            ParticipantSeed left,
            ParticipantSeed right,
            List<CompetitionPairing> pairings,
            IRandomService randomService,
            bool allowExistingLookup = true)
        {
            CompetitionPairing? existing = allowExistingLookup
                ? competition?.CompetitionPairings?
                    .FirstOrDefault(cp => cp.RoundNumber == roundNumber &&
                        NormalizePair(cp.WhitePlayerIdentifierName, cp.BlackPlayerIdentifierName) == NormalizePair(left.IdentifierName, right.IdentifierName))
                : null;

            if (existing != null)
            {
                pairings.Add(new CompetitionPairing { WhitePlayerIdentifierName = existing.WhitePlayerIdentifierName, BlackPlayerIdentifierName = existing.BlackPlayerIdentifierName });
                return;
            }

            bool leftWhite = randomService?.NextBool() ?? true;
            if (leftWhite)
            {
                pairings.Add(new CompetitionPairing { WhitePlayerIdentifierName = left.IdentifierName, BlackPlayerIdentifierName = right.IdentifierName });
            }
            else
            {
                pairings.Add(new CompetitionPairing { WhitePlayerIdentifierName = right.IdentifierName, BlackPlayerIdentifierName = left.IdentifierName });
            }
        }

        private static (string White, string Black) NormalizePair(string firstPlayerId, string secondPlayerId)
        {
            return string.Compare(firstPlayerId, secondPlayerId, StringComparison.OrdinalIgnoreCase) <= 0
                ? (firstPlayerId, secondPlayerId)
                : (secondPlayerId, firstPlayerId);
        }
    }
}

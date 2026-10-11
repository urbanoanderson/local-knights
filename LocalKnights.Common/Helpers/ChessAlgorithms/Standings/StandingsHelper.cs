using System;
using System.Collections.Generic;
using System.Linq;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers
{
    /// <summary>
    /// Provides utility methods for competition standings calculations.
    /// </summary>
    public static class StandingsHelper
    {
        public static IEnumerable<CompetitionStandingEntry> ComputeStandingEntries(Competition competition)
        {
            if (competition == null || competition.CompetitionParticipants == null)
            {
                return [];
            }

            return competition.Type switch
            {
                CompetitionType.KnockoutTournament => ComputeKnockoutStandingEntries(competition),
                _ => ComputeGenericStandingEntries(competition)
            };
        }

        private static IEnumerable<CompetitionStandingEntry> ComputeGenericStandingEntries(Competition competition)
        {
            List<CompetitionStandingEntry> standings = CreateBaseStandingEntries(competition);
            AddStandingPerformanceRatings(competition, standings);
            AddStandingBuchholzValues(competition, standings);
            AddStandingSonnebornBergerValues(competition, standings);
            ClassifyStandings(competition, standings);

            return standings;
        }

        private static IEnumerable<CompetitionStandingEntry> ComputeKnockoutStandingEntries(Competition competition)
        {
            List<CompetitionStandingEntry> standings = CreateBaseStandingEntries(competition);
            AddStandingPerformanceRatings(competition, standings);
            AddStandingBuchholzValuesForPlayedOpponents(competition, standings);
            AddStandingSonnebornBergerValuesForPlayedOpponents(competition, standings);
            ClassifyKnockoutStandings(competition, standings);

            return standings;
        }

        private static List<CompetitionStandingEntry> CreateBaseStandingEntries(Competition competition)
        {
            double byeValue = competition.ByeValue ?? 0d;
            List<CompetitionStandingEntry> standings = new();

            // Create each participant standing entry and count scores
            foreach (CompetitionParticipant cp in competition.CompetitionParticipants)
            {
                standings.Add(new CompetitionStandingEntry()
                {
                    Participant = cp,
                    Score =
                        // Wins
                        ((double)competition.CompetitionPairings.Where(pairing => !pairing.IsByePairing()
                        && (pairing.WhitePlayerIdentifierName == cp.ParticipantIdentifierName && pairing.Result == GameResult.WhiteWins)
                        || (pairing.BlackPlayerIdentifierName == cp.ParticipantIdentifierName && pairing.Result == GameResult.BlackWins)).Count() * CommonConstants.DefaultWinScoreValue)
                        //Draws
                        + ((double)competition.CompetitionPairings.Where(pairing => !pairing.IsByePairing() && pairing.Result == GameResult.Draw
                        && (pairing.WhitePlayerIdentifierName == cp.ParticipantIdentifierName || (pairing.BlackPlayerIdentifierName == cp.ParticipantIdentifierName))).Count() * CommonConstants.DefaultDrawScoreValue)
                        //Byes
                        + ((double)competition.CompetitionPairings.Where(pairing => pairing.IsByePairing() && pairing.WhitePlayerIdentifierName == cp.ParticipantIdentifierName).Count() * byeValue)
                        ,
                    Classification = -1,
                });
            }

            return standings;
        }

        private static void AddStandingBuchholzValues(Competition competition, List<CompetitionStandingEntry> standings)
        {
            if (competition.Type != CompetitionType.SwissSystemTournament)
            {
                return;
            }

            AddStandingBuchholzValuesForPlayedOpponents(competition, standings);
        }

        private static void AddStandingBuchholzValuesForPlayedOpponents(Competition competition, List<CompetitionStandingEntry> standings)
        {
            if (competition.CompetitionPairings == null)
            {
                return;
            }

            Dictionary<string, CompetitionStandingEntry> standingsByParticipant = standings
                .ToDictionary(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase);

            foreach (CompetitionStandingEntry standing in standings)
            {
                standing.BuchholzScore = GetPlayedOpponentIds(competition, standing.Participant.ParticipantIdentifierName)
                    .Where(standingsByParticipant.ContainsKey)
                    .Sum(opponentId => standingsByParticipant[opponentId].Score);
            }
        }

        private static void AddStandingPerformanceRatings(Competition competition, List<CompetitionStandingEntry> standings)
        {
            Dictionary<string, CompetitionParticipant> participantsById = competition.CompetitionParticipants
                .ToDictionary(participant => participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase);

            foreach (CompetitionStandingEntry standing in standings)
            {
                List<CompetitionPairing> playedPairings = competition.CompetitionPairings
                    .Where(pairing => !pairing.IsByePairing() && pairing.Result.HasValue)
                    .Where(pairing => IsPairingForParticipant(pairing, standing.Participant.ParticipantIdentifierName))
                    .ToList();

                if (playedPairings.Count == 0)
                {
                    standing.PerformanceRating = null;
                    continue;
                }

                double opponentRatingAverage = playedPairings
                    .Select(pairing => GetOpponentId(pairing, standing.Participant.ParticipantIdentifierName))
                    .Where(opponentId => !string.IsNullOrWhiteSpace(opponentId) && participantsById.ContainsKey(opponentId))
                    .Select(opponentId => participantsById[opponentId].OldRating.Rating)
                    .DefaultIfEmpty()
                    .Average();

                if (opponentRatingAverage <= 0)
                {
                    standing.PerformanceRating = null;
                    continue;
                }

                double scorePercentage = standing.Score / playedPairings.Count;
                double correctionFactor = InterpolatePerformanceCorrection(scorePercentage);
                standing.PerformanceRating = opponentRatingAverage + correctionFactor;
            }
        }

        private static void AddStandingSonnebornBergerValues(Competition competition, List<CompetitionStandingEntry> standings)
        {
            if (competition.Type != CompetitionType.SwissSystemTournament
                && competition.Type != CompetitionType.RoundRobinTournament)
            {
                return;
            }

            AddStandingSonnebornBergerValuesForPlayedOpponents(competition, standings);
        }

        private static void AddStandingSonnebornBergerValuesForPlayedOpponents(Competition competition, List<CompetitionStandingEntry> standings)
        {
            if (competition.CompetitionPairings == null)
            {
                return;
            }

            Dictionary<string, CompetitionStandingEntry> standingsByParticipant = standings
                .ToDictionary(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase);

            foreach (CompetitionStandingEntry standing in standings)
            {
                standing.SonnebornBergerScore = competition.CompetitionPairings
                    .Where(pairing => !pairing.IsByePairing() && pairing.Result.HasValue)
                    .Where(pairing => IsPairingForParticipant(pairing, standing.Participant.ParticipantIdentifierName))
                    .Sum(pairing => GetSonnebornBergerContribution(pairing, standing.Participant.ParticipantIdentifierName, standingsByParticipant));
            }
        }

        private static void ClassifyStandings(Competition competition, List<CompetitionStandingEntry> standings)
        {
            IOrderedEnumerable<CompetitionStandingEntry> orderedStandings = standings
                .OrderByDescending(entry => entry.Score);

            orderedStandings = competitionTypeSpecificOrder(competition, orderedStandings);

            List<CompetitionStandingEntry> sortedStandings = orderedStandings.ToList();

            standings.Clear();
            standings.AddRange(sortedStandings);

            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Classification = i + 1;
            }
        }

        private static void ClassifyKnockoutStandings(Competition competition, List<CompetitionStandingEntry> standings)
        {
            Dictionary<string, int> classificationByParticipant = BuildKnockoutClassifications(competition);

            List<CompetitionStandingEntry> sortedStandings = standings
                .OrderBy(entry => classificationByParticipant.TryGetValue(entry.Participant.ParticipantIdentifierName, out int classification) ? classification : int.MaxValue)
                .ThenByDescending(entry => entry.Score)
                .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            standings.Clear();
            standings.AddRange(sortedStandings);

            for (int i = 0; i < standings.Count; i++)
            {
                string participantId = standings[i].Participant.ParticipantIdentifierName;
                standings[i].Classification = classificationByParticipant.TryGetValue(participantId, out int classification)
                    ? classification
                    : i + 1;
            }
        }

        private static IOrderedEnumerable<CompetitionStandingEntry> competitionTypeSpecificOrder(
            Competition competition,
            IOrderedEnumerable<CompetitionStandingEntry> orderedStandings,
            List<CompetitionStandingEntry>? standings = null)
        {
            CompetitionType competitionType = competition.Type;

            return competitionType switch
            {
                CompetitionType.SwissSystemTournament => orderedStandings
                    .ThenByDescending(entry => entry.BuchholzScore)
                    .ThenByDescending(entry => entry.SonnebornBergerScore)
                    .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                    .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase),

                CompetitionType.RoundRobinTournament => orderedStandings
                    .ThenByDescending(entry => entry.SonnebornBergerScore)
                    .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                    .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase),

                CompetitionType.KnockoutTournament => orderedStandings
                    .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                    .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase),

                CompetitionType.ChessMatch => orderedStandings
                    .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                    .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase),

                _ => orderedStandings
                    .ThenByDescending(entry => entry.Participant.OldRating.Rating)
                    .ThenBy(entry => entry.Participant.ParticipantIdentifierName, StringComparer.OrdinalIgnoreCase)
            };
        }

        private static Dictionary<string, int> BuildKnockoutClassifications(Competition competition)
        {
            Dictionary<string, int> classificationByParticipant = new(StringComparer.OrdinalIgnoreCase);
            int maxRound = competition.CompetitionPairings.Count == 0
                ? 0
                : competition.CompetitionPairings.Max(pairing => pairing.RoundNumber);

            foreach (CompetitionPairing pairing in competition.CompetitionPairings
                .Where(pairing => !pairing.IsByePairing() && pairing.Result.HasValue)
                .OrderByDescending(pairing => pairing.RoundNumber)
                .ThenBy(pairing => pairing.TableNumber))
            {
                string? winnerId = pairing.Result switch
                {
                    GameResult.WhiteWins => pairing.WhitePlayerIdentifierName,
                    GameResult.BlackWins => pairing.BlackPlayerIdentifierName,
                    _ => null
                };

                string? loserId = pairing.Result switch
                {
                    GameResult.WhiteWins => pairing.BlackPlayerIdentifierName,
                    GameResult.BlackWins => pairing.WhitePlayerIdentifierName,
                    _ => null
                };

                if (string.IsNullOrWhiteSpace(winnerId) || string.IsNullOrWhiteSpace(loserId))
                {
                    continue;
                }

                if (pairing.RoundNumber == maxRound)
                {
                    if (pairing.TableNumber == 1)
                    {
                        if (!classificationByParticipant.ContainsKey(winnerId))
                        {
                            classificationByParticipant[winnerId] = 1;
                        }

                        if (!classificationByParticipant.ContainsKey(loserId))
                        {
                            classificationByParticipant[loserId] = 2;
                        }
                    }
                    else
                    {
                        if (!classificationByParticipant.ContainsKey(winnerId))
                        {
                            classificationByParticipant[winnerId] = 3;
                        }

                        if (!classificationByParticipant.ContainsKey(loserId))
                        {
                            classificationByParticipant[loserId] = 4;
                        }
                    }

                    continue;
                }

                int semifinalBaseClassification = maxRound - pairing.RoundNumber + 2;
                if (!classificationByParticipant.ContainsKey(loserId))
                {
                    classificationByParticipant[loserId] = semifinalBaseClassification;
                }
            }

            return classificationByParticipant;
        }

        private static double InterpolatePerformanceCorrection(double scorePercentage)
        {
            double clampedScorePercentage = Math.Clamp(scorePercentage, 0d, 1d);

            KeyValuePair<double, double>[] performanceTable =
            [
                new(0.0d, -800d),
                new(0.1d, -382d),
                new(0.2d, -240d),
                new(0.3d, -149d),
                new(0.4d, -72d),
                new(0.5d, 0d),
                new(0.6d, 72d),
                new(0.7d, 149d),
                new(0.8d, 240d),
                new(0.9d, 382d),
                new(1.0d, 800d),
            ];

            for (int i = 0; i < performanceTable.Length - 1; i++)
            {
                KeyValuePair<double, double> lower = performanceTable[i];
                KeyValuePair<double, double> upper = performanceTable[i + 1];

                if (clampedScorePercentage < lower.Key || clampedScorePercentage > upper.Key)
                {
                    continue;
                }

                if (Math.Abs(clampedScorePercentage - lower.Key) < double.Epsilon)
                {
                    return lower.Value;
                }

                if (Math.Abs(clampedScorePercentage - upper.Key) < double.Epsilon)
                {
                    return upper.Value;
                }

                double intervalProgress = (clampedScorePercentage - lower.Key) / (upper.Key - lower.Key);
                return lower.Value + ((upper.Value - lower.Value) * intervalProgress);
            }

            return performanceTable[^1].Value;
        }

        private static IEnumerable<string> GetPlayedOpponentIds(Competition competition, string participantId)
        {
            return competition.CompetitionPairings
                .Where(pairing => !pairing.IsByePairing() && pairing.Result.HasValue)
                .Where(pairing => IsPairingForParticipant(pairing, participantId))
                .Select(pairing => GetOpponentId(pairing, participantId));
        }

        private static bool IsPairingForParticipant(CompetitionPairing pairing, string participantId)
        {
            return string.Equals(pairing.WhitePlayerIdentifierName, participantId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(pairing.BlackPlayerIdentifierName, participantId, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetOpponentId(CompetitionPairing pairing, string participantId)
        {
            return string.Equals(pairing.WhitePlayerIdentifierName, participantId, StringComparison.OrdinalIgnoreCase)
                ? pairing.BlackPlayerIdentifierName
                : pairing.WhitePlayerIdentifierName;
        }

        private static double GetSonnebornBergerContribution(
            CompetitionPairing pairing,
            string participantId,
            IReadOnlyDictionary<string, CompetitionStandingEntry> standingsByParticipant)
        {
            string opponentId = GetOpponentId(pairing, participantId);

            if (!standingsByParticipant.TryGetValue(opponentId, out CompetitionStandingEntry? opponentStanding))
            {
                return 0d;
            }

            bool isWhite = string.Equals(pairing.WhitePlayerIdentifierName, participantId, StringComparison.OrdinalIgnoreCase);

            return pairing.Result switch
            {
                GameResult.WhiteWins when isWhite => opponentStanding.Score,
                GameResult.BlackWins when !isWhite => opponentStanding.Score,
                GameResult.Draw => opponentStanding.Score * CommonConstants.DefaultDrawScoreValue,
                _ => 0d
            };
        }
    }
}

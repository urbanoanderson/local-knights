using LocalKnights.Common.Helpers;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;

namespace LocalKnights.Common.Tests.Helpers.ChessAlgorithms.Pairing;

public class KnockoutPairingAlgorithmTests
{
    [Fact]
    public void GeneratePairings_Round1_With8Players_AssignsUniqueSequentialTableNumbers()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 1, numberOfRounds: 3,
            CreatePlayer("A", 2400),
            CreatePlayer("B", 2300),
            CreatePlayer("C", 2200),
            CreatePlayer("D", 2100),
            CreatePlayer("E", 2000),
            CreatePlayer("F", 1900),
            CreatePlayer("G", 1800),
            CreatePlayer("H", 1700));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition, new FixedRandomService(true));

        Assert.Equal([1, 2, 3, 4], pairings.Select(pairing => pairing.TableNumber).ToArray());
        Assert.Equal(4, pairings.Select(pairing => pairing.TableNumber).Distinct().Count());
    }

    [Fact]
    public void GeneratePairings_Round1_With9Players_CreatesFourGamesAndOneBye()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 1, numberOfRounds: 4,
            CreatePlayer("A", 2500),
            CreatePlayer("B", 2400),
            CreatePlayer("C", 2300),
            CreatePlayer("D", 2200),
            CreatePlayer("E", 2100),
            CreatePlayer("F", 2000),
            CreatePlayer("G", 1900),
            CreatePlayer("H", 1800),
            CreatePlayer("I", 1700));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition, new FixedRandomService(true));

        Assert.Equal(5, pairings.Count);
        Assert.Equal(4, pairings.Count(pairing => !pairing.IsByePairing()));
        Assert.Equal(1, pairings.Count(pairing => pairing.IsByePairing()));
        Assert.Contains(pairings, pairing => pairing.IsByePairing() && pairing.WhitePlayerIdentifierName == "A");
    }

    [Fact]
    public void GeneratePairings_Round1_With10Players_CreatesFourGamesAndTwoByes()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 1, numberOfRounds: 4,
            CreatePlayer("A", 2600),
            CreatePlayer("B", 2500),
            CreatePlayer("C", 2400),
            CreatePlayer("D", 2300),
            CreatePlayer("E", 2200),
            CreatePlayer("F", 2100),
            CreatePlayer("G", 2000),
            CreatePlayer("H", 1900),
            CreatePlayer("I", 1800),
            CreatePlayer("J", 1700));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition, new FixedRandomService(true));

        Assert.Equal(6, pairings.Count);
        Assert.Equal(4, pairings.Count(pairing => !pairing.IsByePairing()));
        Assert.Equal(2, pairings.Count(pairing => pairing.IsByePairing()));
        Assert.Contains(pairings, pairing => pairing.IsByePairing() && pairing.WhitePlayerIdentifierName == "A");
        Assert.Contains(pairings, pairing => pairing.IsByePairing() && pairing.WhitePlayerIdentifierName == "B");
    }

    [Fact]
    public void GeneratePairings_FinalRound_With9EqualRatedPlayersAndSemifinalBye_CreatesFinalPairing()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 4, numberOfRounds: 4,
            CreatePlayer("A", 2000),
            CreatePlayer("B", 2000),
            CreatePlayer("C", 2000),
            CreatePlayer("D", 2000),
            CreatePlayer("E", 2000),
            CreatePlayer("F", 2000),
            CreatePlayer("G", 2000),
            CreatePlayer("H", 2000),
            CreatePlayer("I", 2000));

        AddCompletedPairing(competition, 1, -1, "A", string.Empty, null);
        AddCompletedPairing(competition, 1, 1, "B", "I", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 2, "C", "H", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 3, "D", "G", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 4, "E", "F", GameResult.WhiteWins);
        AddCompletedPairing(competition, 2, -1, "A", string.Empty, null);
        AddCompletedPairing(competition, 2, 1, "B", "E", GameResult.WhiteWins);
        AddCompletedPairing(competition, 2, 2, "C", "D", GameResult.WhiteWins);
        AddCompletedPairing(competition, 3, -1, "A", string.Empty, null);
        AddCompletedPairing(competition, 3, 1, "B", "C", GameResult.WhiteWins);

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition, new FixedRandomService(true));

        CompetitionPairing final = Assert.Single(pairings);
        AssertPairing(final, 1, "A", "B");
    }

    [Fact]
    public void GeneratePairings_FinalRound_WithCompletedSemifinals_AddsFinalAndThirdPlacePairings()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 3, numberOfRounds: 3,
            CreatePlayer("A", 2400),
            CreatePlayer("B", 2300),
            CreatePlayer("C", 2200),
            CreatePlayer("D", 2100));

        AddCompletedPairing(competition, 1, 1, "A", string.Empty, null);
        AddCompletedPairing(competition, 1, 2, "B", string.Empty, null);
        AddCompletedPairing(competition, 1, 3, "C", string.Empty, null);
        AddCompletedPairing(competition, 1, 4, "D", string.Empty, null);
        AddCompletedPairing(competition, 2, 1, "A", "B", GameResult.WhiteWins);
        AddCompletedPairing(competition, 2, 2, "C", "D", GameResult.WhiteWins);

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition, new FixedRandomService(true));

        Assert.Collection(pairings.OrderBy(pairing => pairing.TableNumber),
            pairing => AssertPairing(pairing, 1, "A", "C"),
            pairing => AssertPairing(pairing, 2, "B", "D"));
    }

    [Fact]
    public void ComputeStandingEntries_KnockoutFinalRound_ClassifiesChampionRunnerUpThirdAndFourth()
    {
        Competition competition = CreateKnockoutCompetition(currentRound: 3, numberOfRounds: 3,
            CreatePlayer("A", 2400),
            CreatePlayer("B", 2300),
            CreatePlayer("C", 2200),
            CreatePlayer("D", 2100));

        AddCompletedPairing(competition, 1, 1, "A", string.Empty, null);
        AddCompletedPairing(competition, 1, 2, "B", string.Empty, null);
        AddCompletedPairing(competition, 1, 3, "C", string.Empty, null);
        AddCompletedPairing(competition, 1, 4, "D", string.Empty, null);
        AddCompletedPairing(competition, 2, 1, "A", "B", GameResult.WhiteWins);
        AddCompletedPairing(competition, 2, 2, "C", "D", GameResult.WhiteWins);
        AddCompletedPairing(competition, 3, 1, "A", "C", GameResult.WhiteWins);
        AddCompletedPairing(competition, 3, 2, "B", "D", GameResult.WhiteWins);

        List<CompetitionStandingEntry> standings = StandingsHelper.ComputeStandingEntries(competition).ToList();

        Assert.Collection(standings,
            standing => AssertStanding(standing, "A", 1),
            standing => AssertStanding(standing, "C", 2),
            standing => AssertStanding(standing, "B", 3),
            standing => AssertStanding(standing, "D", 4));
    }

    private static List<CompetitionPairing> GenerateRoundPairings(Competition competition, IRandomService randomService)
    {
        return PairingHelper.GenerateCurrentRoundPairings(competition, randomService)
            .OrderBy(pairing => pairing.TableNumber)
            .ToList();
    }

    private static void AssertPairing(CompetitionPairing pairing, int tableNumber, string white, string black)
    {
        Assert.Equal(tableNumber, pairing.TableNumber);
        Assert.Equal(white, pairing.WhitePlayerIdentifierName);
        Assert.Equal(black, pairing.BlackPlayerIdentifierName);
    }

    private static void AssertStanding(CompetitionStandingEntry standing, string participantId, int classification)
    {
        Assert.Equal(participantId, standing.Participant.ParticipantIdentifierName);
        Assert.Equal(classification, standing.Classification);
    }

    private static Competition CreateKnockoutCompetition(int currentRound, int numberOfRounds, params Player[] players)
    {
        Competition competition = new()
        {
            Type = CompetitionType.KnockoutTournament,
            TimeFormat = CommonConstants.DefaultTimeFormat,
            NumberOfRounds = numberOfRounds,
            CurrentRound = currentRound
        };

        foreach (Player player in players)
        {
            competition.CompetitionParticipants.Add(new CompetitionParticipant
            {
                Competition = competition,
                ParticipantIdentifierName = player.IdentifierName,
                OldRating = player.BlitzRating.Clone()
            });
        }

        return competition;
    }

    private static Player CreatePlayer(string identifierName, int blitzRating)
    {
        return new Player
        {
            IdentifierName = identifierName,
            DisplayName = identifierName,
            BlitzRating = new ChessRating { Rating = blitzRating }
        };
    }

    private static void AddCompletedPairing(Competition competition, int roundNumber, int tableNumber, string whitePlayerId, string blackPlayerId, GameResult? result)
    {
        competition.CompetitionPairings.Add(new CompetitionPairing
        {
            Competition = competition,
            RoundNumber = roundNumber,
            TableNumber = tableNumber,
            WhitePlayerIdentifierName = whitePlayerId,
            BlackPlayerIdentifierName = blackPlayerId,
            Result = result,
            GameId = result.HasValue && !string.IsNullOrWhiteSpace(blackPlayerId)
                ? 1
                : null
        });
    }

    private sealed class FixedRandomService(bool nextValue) : IRandomService
    {
        public int Next()
        {
            return 0;
        }

        public int Next(int maxValue)
        {
            return 0;
        }

        public int Next(int minValue, int maxValue)
        {
            return minValue;
        }

        public double NextDouble()
        {
            return 0d;
        }

        public double NextDouble(double minValue, double maxValue)
        {
            return minValue;
        }

        public bool NextBool()
        {
            return nextValue;
        }
    }
}
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using Microsoft.Extensions.Time.Testing;

namespace LocalKnights.Common.Tests.Helpers.ChessAlgorithms.Pairing;

public class SwissPairingAlgorithmTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    [InlineData(8, 7)]
    public void CalculateMaxNumberOfRounds_ReturnsExpectedLimit(int participantCount, int expectedRounds)
    {
        int maxRounds = SwissPairingAlgorithm.CalculateMaxNumberOfRounds(participantCount);

        Assert.Equal(expectedRounds, maxRounds);
    }

    [Fact]
    public void GeneratePairings_Round1_With4Players_UsesDutchSplitPairing()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1,
            CreatePlayer("p1", 2100),
            CreatePlayer("p2", 1900),
            CreatePlayer("p3", 2000),
            CreatePlayer("p4", 1800));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Equal(2, pairings.Count);
        Assert.Contains(pairings, pairing => IsPlayers(pairing, "p1", "p2"));
        Assert.Contains(pairings, pairing => IsPlayers(pairing, "p3", "p4"));
    }

    [Fact]
    public void GeneratePairings_WithSinglePlayer_ReturnsBye()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1, CreatePlayer("p1", 2100));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        CompetitionPairing bye = Assert.Single(pairings);
        AssertPairing(bye, -1, "p1", string.Empty);
    }

    [Fact]
    public void GeneratePairings_PreservesUtf8PlayerNames()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1, CreatePlayer("alirezaostojić505", 2100));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        CompetitionPairing bye = Assert.Single(pairings);
        AssertPairing(bye, -1, "alirezaostojić505", string.Empty);
    }

    [Fact]
    public void GeneratePairings_Golden8Players_Round1_MatchesReferencePairings()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1,
            CreatePlayer("p1", 2400),
            CreatePlayer("p2", 2300),
            CreatePlayer("p3", 2200),
            CreatePlayer("p4", 2100),
            CreatePlayer("p5", 2000),
            CreatePlayer("p6", 1900),
            CreatePlayer("p7", 1800),
            CreatePlayer("p8", 1700));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Collection(pairings.OrderBy(pairing => pairing.TableNumber),
            pairing => AssertPairing(pairing, 1, "p1", "p5"),
            pairing => AssertPairing(pairing, 2, "p6", "p2"),
            pairing => AssertPairing(pairing, 3, "p3", "p7"),
            pairing => AssertPairing(pairing, 4, "p8", "p4"));
    }

    [Fact]
    public void GeneratePairings_Golden8Players_Round2_MatchesReferencePairings()
    {
        Competition competition = CreateSwissCompetition(currentRound: 2,
            CreatePlayer("p1", 2400),
            CreatePlayer("p2", 2300),
            CreatePlayer("p3", 2200),
            CreatePlayer("p4", 2100),
            CreatePlayer("p5", 2000),
            CreatePlayer("p6", 1900),
            CreatePlayer("p7", 1800),
            CreatePlayer("p8", 1700));

        AddCompletedPairing(competition, 1, 1, "p1", "p5", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 2, "p6", "p2", GameResult.BlackWins);
        AddCompletedPairing(competition, 1, 3, "p3", "p7", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 4, "p8", "p4", GameResult.BlackWins);

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        string actual = string.Join(", ", pairings.Select(pairing => $"{pairing.TableNumber}:{pairing.WhitePlayerIdentifierName}-{pairing.BlackPlayerIdentifierName}"));

        Assert.Collection(pairings.OrderBy(pairing => pairing.TableNumber),
            pairing => AssertPairing(pairing, 1, "p4", "p1", actual),
            pairing => AssertPairing(pairing, 2, "p2", "p3", actual),
            pairing => AssertPairing(pairing, 3, "p5", "p8", actual),
            pairing => AssertPairing(pairing, 4, "p7", "p6", actual));
    }

    [Fact]
    public void GeneratePairings_GoldenBasic4Players_Round1_MatchesReferencePairings()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1,
            CreatePlayer("p1", 2100),
            CreatePlayer("p2", 2000),
            CreatePlayer("p3", 1900),
            CreatePlayer("p4", 1800));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Collection(pairings.OrderBy(pairing => pairing.TableNumber),
            pairing => AssertPairing(pairing, 1, "p1", "p3"),
            pairing => AssertPairing(pairing, 2, "p4", "p2"));
    }

    [Fact]
    public void GeneratePairings_GoldenOddPlayersBye_Round1_MatchesReferenceBye()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1,
            CreatePlayer("p1", 2400),
            CreatePlayer("p2", 2300),
            CreatePlayer("p3", 2200),
            CreatePlayer("p4", 2100),
            CreatePlayer("p5", 2000));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Contains(pairings, pairing => pairing.TableNumber == -1 && pairing.WhitePlayerIdentifierName == "p5");
    }

    [Fact]
    public void GeneratePairings_GoldenWithdrawalStyleBye_Round2_MatchesReferenceByeChoice()
    {
        Competition competition = CreateSwissCompetition(currentRound: 2,
            CreatePlayer("p1", 2200),
            CreatePlayer("p2", 2100),
            CreatePlayer("p3", 2000),
            CreatePlayer("p4", 1900),
            CreatePlayer("p5", 1800));

        AddCompletedPairing(competition, 1, 1, "p1", "p4", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 2, "p2", "p5", GameResult.WhiteWins);
        AddCompletedPairing(competition, 1, 3, "p3", string.Empty, null);

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Contains(pairings, pairing => pairing.TableNumber == -1 && pairing.WhitePlayerIdentifierName == "p5");
    }

    [Fact]
    public void GeneratePairings_Golden9Players_Round1_AssignsReferenceByeAndPairings()
    {
        Competition competition = CreateSwissCompetition(currentRound: 1,
            CreatePlayer("p1", 2400),
            CreatePlayer("p2", 2300),
            CreatePlayer("p3", 2200),
            CreatePlayer("p4", 2100),
            CreatePlayer("p5", 2000),
            CreatePlayer("p6", 1900),
            CreatePlayer("p7", 1800),
            CreatePlayer("p8", 1700),
            CreatePlayer("p9", 1600));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        Assert.Contains(pairings, pairing => pairing.TableNumber == -1 && pairing.WhitePlayerIdentifierName == "p9");
        Assert.Contains(pairings, pairing => pairing.TableNumber == 1 && pairing.WhitePlayerIdentifierName == "p1" && pairing.BlackPlayerIdentifierName == "p5");
        Assert.Contains(pairings, pairing => pairing.TableNumber == 2 && pairing.WhitePlayerIdentifierName == "p6" && pairing.BlackPlayerIdentifierName == "p2");
        Assert.Contains(pairings, pairing => pairing.TableNumber == 3 && pairing.WhitePlayerIdentifierName == "p3" && pairing.BlackPlayerIdentifierName == "p7");
        Assert.Contains(pairings, pairing => pairing.TableNumber == 4 && pairing.WhitePlayerIdentifierName == "p8" && pairing.BlackPlayerIdentifierName == "p4");
    }

    private static List<CompetitionPairing> GenerateRoundPairings(Competition competition)
    {
        return PairingHelper.GenerateCurrentRoundPairings(competition, new RandomService(new FakeTimeProvider()))
            .OrderBy(pairing => pairing.TableNumber == -1 ? int.MaxValue : pairing.TableNumber)
            .ToList();
    }

    private static void AssertPairing(CompetitionPairing pairing, int board, string white, string black, string? message = null)
    {
        Assert.True(pairing.TableNumber == board, message ?? $"Expected board {board} but got {pairing.TableNumber}");
        Assert.True(pairing.WhitePlayerIdentifierName == white, message ?? $"Expected white {white} but got {pairing.WhitePlayerIdentifierName}");
        Assert.True(pairing.BlackPlayerIdentifierName == black, message ?? $"Expected black {black} but got {pairing.BlackPlayerIdentifierName}");
    }

    private static bool IsPlayers(CompetitionPairing pairing, string first, string second)
    {
        return (pairing.WhitePlayerIdentifierName == first && pairing.BlackPlayerIdentifierName == second)
            || (pairing.WhitePlayerIdentifierName == second && pairing.BlackPlayerIdentifierName == first);
    }

    private static Competition CreateSwissCompetition(int currentRound, params Player[] players)
    {
        Competition competition = new()
        {
            Type = CompetitionType.SwissSystemTournament,
            TimeFormat = CommonConstants.DefaultTimeFormat,
            ByeValue = CommonConstants.DefaultByeValue,
            NumberOfRounds = 5,
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
}

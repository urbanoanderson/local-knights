using LocalKnights.Common.Helpers;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using Microsoft.Extensions.Time.Testing;

namespace LocalKnights.Common.Tests.Helpers.ChessAlgorithms.Pairing;

public class RoundRobinPairingAlgorithmTests
{
    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(0, true, 0)]
    [InlineData(1, false, 0)]
    [InlineData(1, true, 0)]
    [InlineData(2, false, 1)]
    [InlineData(2, true, 2)]
    [InlineData(3, false, 3)]
    [InlineData(3, true, 6)]
    public void CalculateNumberOfRounds_ReturnsExpectedValue(int participantCount, bool isDoubleRoundRobin, int expectedRounds)
    {
        int rounds = RoundRobinPairingAlgorithm.CalculateNumberOfRounds(participantCount, isDoubleRoundRobin);

        Assert.Equal(expectedRounds, rounds);
    }

    [Fact]
    public void GeneratePairings_Round1_With3Players_GeneratesAByeAndOneGame()
    {
        Competition competition = CreateRoundRobinCompetition(
            currentRound: 1,
            isDoubleRoundRobin: false,
            CreatePlayer("p1", 2100),
            CreatePlayer("p2", 2000),
            CreatePlayer("p3", 1900));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        CompetitionPairing bye = Assert.Single(pairings.Where(pairing => pairing.TableNumber == -1));
        CompetitionPairing game = Assert.Single(pairings.Where(pairing => pairing.TableNumber != -1));

        Assert.Equal(string.Empty, bye.BlackPlayerIdentifierName);
        Assert.NotEqual(string.Empty, bye.WhitePlayerIdentifierName);
        Assert.NotEqual(bye.WhitePlayerIdentifierName, game.WhitePlayerIdentifierName);
        Assert.NotEqual(bye.WhitePlayerIdentifierName, game.BlackPlayerIdentifierName);
        Assert.Equal(2, pairings.Count);
    }

    [Fact]
    public void GeneratePairings_DoubleRoundRobin_With3Players_GeneratesAByeAndOneGame()
    {
        Competition competition = CreateRoundRobinCompetition(
            currentRound: 1,
            isDoubleRoundRobin: true,
            CreatePlayer("p1", 2100),
            CreatePlayer("p2", 2000),
            CreatePlayer("p3", 1900));

        List<CompetitionPairing> pairings = GenerateRoundPairings(competition);

        CompetitionPairing bye = Assert.Single(pairings.Where(pairing => pairing.TableNumber == -1));
        CompetitionPairing game = Assert.Single(pairings.Where(pairing => pairing.TableNumber != -1));

        Assert.Equal(string.Empty, bye.BlackPlayerIdentifierName);
        Assert.NotEqual(string.Empty, bye.WhitePlayerIdentifierName);
        Assert.NotEqual(bye.WhitePlayerIdentifierName, game.WhitePlayerIdentifierName);
        Assert.NotEqual(bye.WhitePlayerIdentifierName, game.BlackPlayerIdentifierName);
        Assert.Equal(2, pairings.Count);
    }

    private static List<CompetitionPairing> GenerateRoundPairings(Competition competition)
    {
        return PairingHelper.GenerateCurrentRoundPairings(competition, new RandomService(new FakeTimeProvider()))
            .OrderBy(pairing => pairing.TableNumber == -1 ? int.MaxValue : pairing.TableNumber)
            .ToList();
    }

    private static bool IsPlayers(CompetitionPairing pairing, string first, string second)
    {
        return (pairing.WhitePlayerIdentifierName == first && pairing.BlackPlayerIdentifierName == second)
            || (pairing.WhitePlayerIdentifierName == second && pairing.BlackPlayerIdentifierName == first);
    }

    private static Competition CreateRoundRobinCompetition(int currentRound, bool isDoubleRoundRobin, params Player[] players)
    {
        Competition competition = new()
        {
            Type = CompetitionType.RoundRobinTournament,
            TimeFormat = CommonConstants.DefaultTimeFormat,
            NumberOfRounds = RoundRobinPairingAlgorithm.CalculateNumberOfRounds(players.Length, isDoubleRoundRobin),
            CurrentRound = currentRound,
            IsDoubleRoundRobin = isDoubleRoundRobin,
        };

        foreach (Player player in players)
        {
            competition.CompetitionParticipants.Add(new CompetitionParticipant
            {
                Competition = competition,
                ParticipantIdentifierName = player.IdentifierName,
                OldRating = player.BlitzRating,
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
}
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using Moq;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Tests TRF16 fixed-width export behavior.
/// </summary>
public sealed class ExportToTRFHelperTests
{
    /// <summary>
    /// Verifies that a completed competition is converted to tournament and player TRF16 records.
    /// </summary>
    [Fact]
    public async Task ExportCompetitionToTrf16_WithCompletedCompetition_ConvertsRelatedEntities()
    {
        CompetitionParticipant alphaParticipant = CreateParticipant("alpha", 2200.4);
        CompetitionParticipant zuluParticipant = CreateParticipant("zulu", 2200.4);
        CompetitionParticipant deletedParticipant = CreateParticipant("deleted", 1800.6);
        Competition competition = new Competition
        {
            Title = "Club Final",
            City = "Madrid",
            Federation = "ESP",
            ChiefArbiter = "Arbiter Name",
            State = CompetitionState.Completed,
            Type = CompetitionType.SwissSystemTournament,
            TimeFormat = TimeFormat.Classical,
            StartedAt = new DateTime(2026, 4, 1),
            CompletedAt = new DateTime(2026, 4, 8),
            NumberOfRounds = 2,
            ByeValue = 0.5d,
            CompetitionParticipants = [zuluParticipant, deletedParticipant, alphaParticipant],
            CompetitionPairings =
            [
                new CompetitionPairing
                {
                    RoundNumber = 1,
                    TableNumber = 1,
                    WhitePlayerIdentifierName = "alpha",
                    BlackPlayerIdentifierName = "zulu",
                    Result = GameResult.WhiteWins,
                },
                new CompetitionPairing
                {
                    RoundNumber = 1,
                    TableNumber = -1,
                    WhitePlayerIdentifierName = "deleted",
                },
                new CompetitionPairing
                {
                    RoundNumber = 2,
                    TableNumber = 1,
                    WhitePlayerIdentifierName = "zulu",
                    BlackPlayerIdentifierName = "deleted",
                    Result = GameResult.Draw,
                },
                new CompetitionPairing
                {
                    RoundNumber = 2,
                    TableNumber = -1,
                    WhitePlayerIdentifierName = "alpha",
                },
            ],
            ComputedStandings =
            [
                CreateStanding(alphaParticipant, 1.5d, 1),
                CreateStanding(zuluParticipant, 0.5d, 2),
                CreateStanding(deletedParticipant, 1d, 3),
            ],
        };
        Dictionary<string, Player> players = new(StringComparer.OrdinalIgnoreCase)
        {
            ["alpha"] = new Player
            {
                IdentifierName = "alpha",
                DisplayName = "Alpha Player",
                ChessTitle = ChessTitle.NM,
            },
            ["zulu"] = new Player
            {
                IdentifierName = "zulu",
                DisplayName = "Zulu Player",
                ChessTitle = ChessTitle.GM,
                FideId = "1234567",
                BirthDate = new DateTime(1990, 2, 3),
            },
        };
        Mock<IPlayerService> playerService = new();
        playerService
            .Setup(service => service.GetPlayerByIdentifierAsync(It.IsAny<string>()))
            .ReturnsAsync((string identifier) => players.GetValueOrDefault(identifier));

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(competition, playerService.Object);
        string[] lines = result.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        string roundDatesLine = Assert.Single(lines, line => line.StartsWith("132", StringComparison.Ordinal));
        string alphaLine = FindPlayerLine(lines, "Alpha Player");
        string zuluLine = FindPlayerLine(lines, "Zulu Player");
        string deletedLine = FindPlayerLine(lines, "deleted");

        Assert.Contains("012 Club Final", lines);
        Assert.Contains("022 Madrid", lines);
        Assert.Contains("032 ESP", lines);
        Assert.Contains("042 2026/04/01", lines);
        Assert.Contains("052 2026/04/08", lines);
        Assert.Contains("062 3", lines);
        Assert.Contains("072 0", lines);
        Assert.Contains("082 0", lines);
        Assert.Contains("092 Swiss-System Dutch", lines);
        Assert.Contains("102 Arbiter Name", lines);
        Assert.Equal("26/04/08", roundDatesLine.Substring(91, 8));
        Assert.Equal("26/04/08", roundDatesLine.Substring(101, 8));

        Assert.Equal("   1", alphaLine.Substring(4, 4));
        Assert.Equal("   ", alphaLine.Substring(10, 3));
        Assert.Equal("2200", alphaLine.Substring(48, 4));
        Assert.Equal(" 1.5", alphaLine.Substring(80, 4));
        Assert.Equal("   1", alphaLine.Substring(85, 4));
        Assert.Equal("0002", alphaLine.Substring(91, 4));
        Assert.Equal('w', alphaLine[96]);
        Assert.Equal('1', alphaLine[98]);
        Assert.Equal("0000", alphaLine.Substring(101, 4));
        Assert.Equal('H', alphaLine[108]);

        Assert.Equal("   2", zuluLine.Substring(4, 4));
        Assert.Equal("GM ", zuluLine.Substring(10, 3));
        Assert.Equal("    1234567", zuluLine.Substring(57, 11));
        Assert.Equal("1990/02/03", zuluLine.Substring(69, 10));
        Assert.Equal('b', zuluLine[96]);
        Assert.Equal('0', zuluLine[98]);
        Assert.Equal("0003", zuluLine.Substring(101, 4));
        Assert.Equal('w', zuluLine[106]);
        Assert.Equal('=', zuluLine[108]);

        Assert.Equal("   3", deletedLine.Substring(4, 4));
        Assert.Equal("deleted", deletedLine.Substring(14, 33).TrimEnd());
        Assert.Equal("1801", deletedLine.Substring(48, 4));
        Assert.Equal("           ", deletedLine.Substring(57, 11));
        Assert.Equal('H', deletedLine[98]);
        Assert.Equal("0002", deletedLine.Substring(101, 4));
        Assert.Equal('b', deletedLine[106]);
        Assert.Equal('=', deletedLine[108]);
        playerService.Verify(
            service => service.GetPlayerByIdentifierAsync(It.IsAny<string>()),
            Times.Exactly(3));
    }

    /// <summary>
    /// Verifies every supported competition type is converted to its TRF16 description.
    /// </summary>
    [Theory]
    [InlineData(CompetitionType.SwissSystemTournament, false, "Swiss-System Dutch")]
    [InlineData(CompetitionType.KnockoutTournament, false, "Knockout")]
    [InlineData(CompetitionType.RoundRobinTournament, false, "Round-Robin")]
    [InlineData(CompetitionType.RoundRobinTournament, true, "Double Round-Robin")]
    [InlineData(CompetitionType.ChessMatch, false, "Chess Match")]
    public async Task ExportCompetitionToTrf16_WithCompetitionType_UsesExpectedDescription(
        CompetitionType competitionType,
        bool isDoubleRoundRobin,
        string expectedDescription)
    {
        Competition competition = new Competition
        {
            Title = "Tournament",
            State = CompetitionState.Completed,
            Type = competitionType,
            TimeFormat = TimeFormat.Blitz,
            IsDoubleRoundRobin = isDoubleRoundRobin,
        };
        Mock<IPlayerService> playerService = new();

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(competition, playerService.Object);

        Assert.Contains($"092 {expectedDescription}\r", result, StringComparison.Ordinal);
        Assert.Contains("022 Unknown\r", result, StringComparison.Ordinal);
        Assert.Contains("032 Unknown\r", result, StringComparison.Ordinal);
        Assert.Contains("102 Unknown\r", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies competitions that are not completed return before querying players.
    /// </summary>
    [Fact]
    public async Task ExportCompetitionToTrf16_WithIncompleteCompetition_ReturnsEmptyString()
    {
        Competition competition = new Competition
        {
            Title = "Ongoing Tournament",
            State = CompetitionState.Ongoing,
            TimeFormat = TimeFormat.Rapid,
        };
        Mock<IPlayerService> playerService = new();

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(competition, playerService.Object);

        Assert.Empty(result);
        playerService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Verifies tournament records, player columns, round blocks, and CR terminators.
    /// </summary>
    [Fact]
    public void ExportCompetitionToTrf16_WithCompleteData_UsesTrf16Positions()
    {
        Trf16Tournament tournament = new Trf16Tournament
        {
            TournamentName = "City Championship",
            City = "Madrid",
            Federation = "ESP",
            StartDate = new DateTime(2026, 4, 8),
            EndDate = new DateTime(2026, 4, 15),
            NumberOfPlayers = 2,
            NumberOfRatedPlayers = 2,
            NumberOfTeams = 0,
            TournamentType = "Individual: Swiss-System Dutch",
            ChiefArbiter = "Chief Arbiter",
            DeputyChiefArbiters = ["Deputy One", "Deputy Two"],
            AllottedTime = "90 minutes + 30 seconds per move",
            RoundDates = [new DateTime(2026, 4, 8), new DateTime(2026, 4, 15)],
        };
        Trf16Player player = new Trf16Player
        {
            StartingRankNumber = 2,
            Sex = "m",
            Title = "FM",
            Name = "Doe, John",
            FideRating = 2150,
            FideFederation = "ESP",
            FideNumber = "1234567",
            BirthDate = new DateTime(1990, 2, 3),
            Points = 1.5m,
            Rank = 1,
            Rounds =
            [
                new Trf16PlayerRound
                {
                    OpponentStartingRankNumber = 1,
                    Color = "w",
                    Result = "1",
                },
                new Trf16PlayerRound
                {
                    OpponentStartingRankNumber = 0,
                    Color = "-",
                    Result = "H",
                },
            ],
        };

        string result = ExportToTRFHelper.ExportCompetitionToTrf16(tournament, [player]);
        string[] lines = result.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        string roundDatesLine = Assert.Single(lines, line => line.StartsWith("132", StringComparison.Ordinal));
        string playerLine = Assert.Single(lines, line => line.StartsWith("001", StringComparison.Ordinal));

        Assert.DoesNotContain('\n', result);
        Assert.EndsWith("\r", result, StringComparison.Ordinal);
        Assert.Contains("012 City Championship", lines);
        Assert.Contains("042 2026/04/08", lines);
        Assert.Contains("052 2026/04/15", lines);
        Assert.Equal(2, lines.Count(line => line.StartsWith("112 ", StringComparison.Ordinal)));
        Assert.Equal("26/04/08", roundDatesLine.Substring(91, 8));
        Assert.Equal("26/04/15", roundDatesLine.Substring(101, 8));

        Assert.Equal(109, playerLine.Length);
        Assert.Equal("001", playerLine[..3]);
        Assert.Equal("   2", playerLine.Substring(4, 4));
        Assert.Equal('m', playerLine[9]);
        Assert.Equal("FM ", playerLine.Substring(10, 3));
        Assert.Equal("Doe, John", playerLine.Substring(14, 33).TrimEnd());
        Assert.Equal("2150", playerLine.Substring(48, 4));
        Assert.Equal("ESP", playerLine.Substring(53, 3));
        Assert.Equal("    1234567", playerLine.Substring(57, 11));
        Assert.Equal("1990/02/03", playerLine.Substring(69, 10));
        Assert.Equal(" 1.5", playerLine.Substring(80, 4));
        Assert.Equal("   1", playerLine.Substring(85, 4));
        Assert.Equal("0001", playerLine.Substring(91, 4));
        Assert.Equal('w', playerLine[96]);
        Assert.Equal('1', playerLine[98]);
        Assert.Equal("0000", playerLine.Substring(101, 4));
        Assert.Equal('-', playerLine[106]);
        Assert.Equal('H', playerLine[108]);
    }

    /// <summary>
    /// Verifies blank optional fields, fixed-width truncation, and embedded newline sanitization.
    /// </summary>
    [Fact]
    public void ExportCompetitionToTrf16_WithOptionalAndLongValues_ProducesValidSingleLineRecords()
    {
        Trf16Tournament tournament = new Trf16Tournament
        {
            TournamentName = "Line one\r\nLine two",
        };
        Trf16Player player = new Trf16Player
        {
            StartingRankNumber = 1,
            Name = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
        };

        string result = ExportToTRFHelper.ExportCompetitionToTrf16(tournament, [player]);
        string[] lines = result.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        string playerLine = Assert.Single(lines, line => line.StartsWith("001", StringComparison.Ordinal));

        Assert.Contains("012 Line one  Line two", lines);
        Assert.Contains("112 ", lines);
        Assert.Equal(89, playerLine.Length);
        Assert.Equal("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567", playerLine.Substring(14, 33));
        Assert.Equal("    ", playerLine.Substring(48, 4));
        Assert.Equal("          ", playerLine.Substring(69, 10));
        Assert.Equal("    ", playerLine.Substring(80, 4));
    }

    /// <summary>
    /// Verifies null arguments are rejected with useful parameter names.
    /// </summary>
    [Fact]
    public void ExportCompetitionToTrf16_WithNullArguments_ThrowsArgumentNullException()
    {
        ArgumentNullException tournamentException = Assert.Throws<ArgumentNullException>(
            () => ExportToTRFHelper.ExportCompetitionToTrf16(null!, []));
        ArgumentNullException playersException = Assert.Throws<ArgumentNullException>(
            () => ExportToTRFHelper.ExportCompetitionToTrf16(new Trf16Tournament(), null!));

        Assert.Equal("tournament", tournamentException.ParamName);
        Assert.Equal("players", playersException.ParamName);
    }

    private static CompetitionParticipant CreateParticipant(string identifierName, double rating)
    {
        return new CompetitionParticipant
        {
            ParticipantIdentifierName = identifierName,
            OldRating = new ChessRating { Rating = rating },
        };
    }

    private static CompetitionStandingEntry CreateStanding(
        CompetitionParticipant participant,
        double score,
        int classification)
    {
        return new CompetitionStandingEntry
        {
            Participant = participant,
            Score = score,
            Classification = classification,
        };
    }

    private static string FindPlayerLine(IEnumerable<string> lines, string playerName)
    {
        return Assert.Single(
            lines,
            line => line.StartsWith("001", StringComparison.Ordinal)
                && string.Equals(
                    line.Substring(14, 33).TrimEnd(),
                    playerName,
                    StringComparison.Ordinal));
    }
}
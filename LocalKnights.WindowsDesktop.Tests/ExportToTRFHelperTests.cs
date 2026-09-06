using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
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
                FideId = " ",
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
        Mock<IFideApiService> fideApiService = CreateFideApiServiceMock();
        fideApiService
            .Setup(service => service.GetPlayerRatingsAsync("1234567"))
            .ReturnsAsync(new Dictionary<TimeFormat, double>
            {
                [TimeFormat.Classical] = 2468.6d,
            });

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            new ClubPreferences(),
            playerService.Object,
            fideApiService.Object);
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
        Assert.Contains("072 1", lines);
        Assert.Contains("082 0", lines);
        Assert.Contains("092 Swiss-System Dutch", lines);
        Assert.Contains("102 Arbiter Name", lines);
        Assert.Equal("132 2026/04/08 2026/04/08", roundDatesLine);

        Assert.Equal("   1", alphaLine.Substring(4, 4));
        Assert.Equal("   ", alphaLine.Substring(10, 3));
        Assert.Equal("   0", alphaLine.Substring(48, 4));
        Assert.Equal(" 1.5", alphaLine.Substring(80, 4));
        Assert.Equal("   1", alphaLine.Substring(85, 4));
        Assert.Equal("   2", alphaLine.Substring(91, 4));
        Assert.Equal('w', alphaLine[96]);
        Assert.Equal('1', alphaLine[98]);
        Assert.Equal("0000", alphaLine.Substring(101, 4));
        Assert.Equal('H', alphaLine[108]);

        Assert.Equal("   2", zuluLine.Substring(4, 4));
        Assert.Equal(" GM", zuluLine.Substring(10, 3));
        Assert.Equal("2469", zuluLine.Substring(48, 4));
        Assert.Equal("    1234567", zuluLine.Substring(57, 11));
        Assert.Equal("1990/02/03", zuluLine.Substring(69, 10));
        Assert.Equal('b', zuluLine[96]);
        Assert.Equal('0', zuluLine[98]);
        Assert.Equal("   3", zuluLine.Substring(101, 4));
        Assert.Equal('w', zuluLine[106]);
        Assert.Equal('=', zuluLine[108]);

        Assert.Equal("   3", deletedLine.Substring(4, 4));
        Assert.Equal(' ', deletedLine[9]);
        Assert.Equal("deleted", deletedLine.Substring(14, 33).TrimEnd());
        Assert.Equal("   0", deletedLine.Substring(48, 4));
        Assert.Equal("           ", deletedLine.Substring(57, 11));
        Assert.Equal('H', deletedLine[98]);
        Assert.Equal("   2", deletedLine.Substring(101, 4));
        Assert.Equal('b', deletedLine[106]);
        Assert.Equal('=', deletedLine[108]);
        playerService.Verify(
            service => service.GetPlayerByIdentifierAsync(It.IsAny<string>()),
            Times.Exactly(3));
        fideApiService.Verify(service => service.GetPlayerRatingsAsync("1234567"), Times.Once);
        fideApiService.VerifyNoOtherCalls();
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

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            new ClubPreferences(),
            playerService.Object,
            CreateFideApiServiceMock().Object);

        Assert.Contains($"092 {expectedDescription}\r", result, StringComparison.Ordinal);
        Assert.Contains("022 Unknown\r", result, StringComparison.Ordinal);
        Assert.Contains("032 Unknown\r", result, StringComparison.Ordinal);
        Assert.Contains("102 Unknown\r", result, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExportCompetitionToTrf16_WithMissingCompetitionMetadata_UsesClubDefaults()
    {
        Competition competition = new Competition
        {
            Title = "Tournament",
            City = " ",
            Federation = null,
            ChiefArbiter = string.Empty,
            State = CompetitionState.Completed,
            TimeFormat = TimeFormat.Blitz,
        };
        ClubPreferences clubPreferences = new()
        {
            City = "Recife",
            Federation = "BRA",
            DefaultChiefArbiter = "Alex Silva",
        };

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            clubPreferences,
            new Mock<IPlayerService>().Object,
            CreateFideApiServiceMock().Object);

        Assert.Contains("022 Recife\r", result, StringComparison.Ordinal);
        Assert.Contains("032 BRA\r", result, StringComparison.Ordinal);
        Assert.Contains("102 Alex Silva\r", result, StringComparison.Ordinal);
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
        Mock<IFideApiService> fideApiService = CreateFideApiServiceMock();

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            new ClubPreferences(),
            playerService.Object,
            fideApiService.Object);

        Assert.Empty(result);
        playerService.VerifyNoOtherCalls();
        fideApiService.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Verifies player genders are converted to the TRF16 sex codes.
    /// </summary>
    [Theory]
    [InlineData(GenderType.Male, 'm')]
    [InlineData(GenderType.Female, 'w')]
    [InlineData(GenderType.Other, ' ')]
    public async Task ExportCompetitionToTrf16_WithPlayerGender_UsesExpectedSexCode(
        GenderType gender,
        char expectedSexCode)
    {
        CompetitionParticipant participant = CreateParticipant("player", 1500d);
        Competition competition = new Competition
        {
            Title = "Tournament",
            State = CompetitionState.Completed,
            TimeFormat = TimeFormat.Blitz,
            CompetitionParticipants = [participant],
        };
        Player player = new Player
        {
            IdentifierName = "player",
            DisplayName = "Player",
            Gender = gender,
        };
        Mock<IPlayerService> playerService = new();
        playerService
            .Setup(service => service.GetPlayerByIdentifierAsync("player"))
            .ReturnsAsync(player);

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            new ClubPreferences(),
            playerService.Object,
            CreateFideApiServiceMock().Object);
        string playerLine = FindPlayerLine(
            result.Split('\r', StringSplitOptions.RemoveEmptyEntries),
            "Player");

        Assert.Equal(expectedSexCode, playerLine[9]);
    }

    /// <summary>
    /// Verifies a player is unrated when FIDE has no rating for the competition time format.
    /// </summary>
    [Fact]
    public async Task ExportCompetitionToTrf16_WithoutMatchingFideTimeFormat_UsesZeroRating()
    {
        CompetitionParticipant participant = CreateParticipant("player", 2100d);
        Competition competition = new Competition
        {
            Title = "Rapid Tournament",
            State = CompetitionState.Completed,
            TimeFormat = TimeFormat.Rapid,
            CompetitionParticipants = [participant],
        };
        Player player = new Player
        {
            IdentifierName = "player",
            DisplayName = "Player",
            FideId = "7654321",
        };
        Mock<IPlayerService> playerService = new();
        playerService
            .Setup(service => service.GetPlayerByIdentifierAsync("player"))
            .ReturnsAsync(player);
        Mock<IFideApiService> fideApiService = CreateFideApiServiceMock();
        fideApiService
            .Setup(service => service.GetPlayerRatingsAsync("7654321"))
            .ReturnsAsync(new Dictionary<TimeFormat, double>
            {
                [TimeFormat.Blitz] = 1900d,
            });

        string result = await ExportToTRFHelper.ExportCompetitionToTrf16(
            competition,
            new ClubPreferences(),
            playerService.Object,
            fideApiService.Object);
        string playerLine = FindPlayerLine(
            result.Split('\r', StringSplitOptions.RemoveEmptyEntries),
            "Player");

        Assert.Contains("072 0\r", result, StringComparison.Ordinal);
        Assert.Equal("   0", playerLine.Substring(48, 4));
        fideApiService.Verify(service => service.GetPlayerRatingsAsync("7654321"), Times.Once);
    }

    /// <summary>
    /// Verifies two-character titles are right-aligned while three-character titles fill the TRF title field.
    /// </summary>
    [Theory]
    [InlineData("m", "GM", "m GM")]
    [InlineData("w", "WGM", "wWGM")]
    public void ExportCompetitionToTrf16_WithTitle_UsesReferenceTitleLayout(
        string sex,
        string title,
        string expectedLayout)
    {
        Trf16Player player = new Trf16Player
        {
            StartingRankNumber = 1,
            Sex = sex,
            Title = title,
            Name = "Player",
        };

        string result = ExportToTRFHelper.ExportCompetitionToTrf16(new Trf16Tournament(), [player]);
        string playerLine = Assert.Single(
            result.Split('\r', StringSplitOptions.RemoveEmptyEntries),
            line => line.StartsWith("001", StringComparison.Ordinal));

        Assert.Equal(expectedLayout, playerLine.Substring(9, 4));
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
        Assert.Equal("132 2026/04/08 2026/04/15", roundDatesLine);

        Assert.Equal(109, playerLine.Length);
        Assert.Equal("001", playerLine[..3]);
        Assert.Equal("   2", playerLine.Substring(4, 4));
        Assert.Equal('m', playerLine[9]);
        Assert.Equal(" FM", playerLine.Substring(10, 3));
        Assert.Equal("Doe, John", playerLine.Substring(14, 33).TrimEnd());
        Assert.Equal("2150", playerLine.Substring(48, 4));
        Assert.Equal("ESP", playerLine.Substring(53, 3));
        Assert.Equal("    1234567", playerLine.Substring(57, 11));
        Assert.Equal("1990/02/03", playerLine.Substring(69, 10));
        Assert.Equal(" 1.5", playerLine.Substring(80, 4));
        Assert.Equal("   1", playerLine.Substring(85, 4));
        Assert.Equal("   1", playerLine.Substring(91, 4));
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

    private static Mock<IFideApiService> CreateFideApiServiceMock()
    {
        Mock<IFideApiService> mock = new();
        mock.Setup(service => service.GetPlayerRatingsAsync(It.IsAny<string>()))
            .ReturnsAsync(new Dictionary<TimeFormat, double>());
        return mock;
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
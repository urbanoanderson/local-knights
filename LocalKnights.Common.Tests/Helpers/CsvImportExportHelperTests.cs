using System.Globalization;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using Moq;

namespace LocalKnights.Common.Tests.Helpers;

public sealed class CsvImportExportHelperTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (string filePath in _tempFiles)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task ExportPlayersToCsv_WritesHeaderAndPlayerRowsUsingImportSchema()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            string filePath = CreateTempCsvPath();
            List<Player> players =
            [
                new Player
                {
                    IdentifierName = "alpha1",
                    DisplayName = "Alpha One",
                    LichessUsername = "alpha-lichess",
                    ChessComUsername = "alpha-chesscom",
                    Email = "alpha@example.com",
                    Phone = "555-0101",
                    FideId = "123456",
                    CbxId = "654321",
                    UscfId = "111222",
                    ChessTitle = ChessTitle.FM,
                    BirthDate = new DateTime(1990, 4, 20),
                    BulletRating = new ChessRating { Rating = 1800, Deviation = 45, Volatility = 0.06 },
                    BlitzRating = new ChessRating { Rating = 1810, Deviation = 46, Volatility = 0.06 },
                    RapidRating = new ChessRating { Rating = 1820, Deviation = 47, Volatility = 0.06 },
                    ClassicalRating = new ChessRating { Rating = 1830, Deviation = 48, Volatility = 0.06 }
                }
            ];

            await CsvImportExportHelper.ExportPlayersToCsv(filePath, players);

            string[] lines = await File.ReadAllLinesAsync(filePath);

            Assert.Equal(2, lines.Length);
            Assert.Equal("IdentifierName,DisplayName,LichessUsername,ChessComUsername,Email,Phone,FideId,CbxId,UscfId,ChessTitle,BirthDate,BulletRating,BulletDeviation,BlitzRating,BlitzDeviation,RapidRating,RapidDeviation,ClassicalRating,ClassicalDeviation", lines[0]);
            Assert.Equal("alpha1,Alpha One,alpha-lichess,alpha-chesscom,alpha@example.com,555-0101,123456,654321,111222,FM,04/20/1990,1800,45,1810,46,1820,47,1830,48", lines[1]);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task ImportPlayersFromCsv_WithRequiredColumnsOnly_ImportsValidPlayers()
    {
        string filePath = CreateTempCsv(
            "IdentifierName,DisplayName",
            "alpha1,Alpha One",
            "bravo2,Bravo Two");

        Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock();

        (List<Player> validPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences: null);

        Assert.Equal(2, validPlayers.Count);
        Assert.Equal(0, invalidCount);

        Assert.Equal("alpha1", validPlayers[0].IdentifierName);
        Assert.Equal("Alpha One", validPlayers[0].DisplayName);
        Assert.Null(validPlayers[0].LichessUsername);

        Assert.Equal("bravo2", validPlayers[1].IdentifierName);
        Assert.Equal("Bravo Two", validPlayers[1].DisplayName);
    }

    [Fact]
    public async Task ImportPlayersFromCsv_WithOptionalColumns_MapsValuesAndUsesDefaultDeviation()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            string filePath = CreateTempCsv(
                "IdentifierName,DisplayName,ChessTitle,BirthDate,LichessUsername,BulletRating,BlitzRating,RapidRating,ClassicalRating",
                "Alpha1,Alpha One,FM,1990-04-20,alpha-lichess,1800,1810,1820,1830");

            Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock();
            ClubPreferences clubPreferences = new ClubPreferences
            {
                Deviation = 72,
                Volatility = 0.09,
            };

            (List<Player> validPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences);

            Player player = Assert.Single(validPlayers);
            Assert.Equal(0, invalidCount);
            Assert.Equal("alpha1", player.IdentifierName);
            Assert.Equal(ChessTitle.FM, player.ChessTitle);
            Assert.Equal(new DateTime(1990, 4, 20), player.BirthDate);
            Assert.Equal("alpha-lichess", player.LichessUsername);
            Assert.Equal(1800, player.BulletRating.Rating);
            Assert.Equal(72, player.BulletRating.Deviation);
            Assert.Equal(0.09, player.BulletRating.Volatility);
            Assert.Equal(1830, player.ClassicalRating.Rating);
            Assert.Equal(72, player.ClassicalRating.Deviation);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task ImportPlayersFromCsv_WhenRowIsDuplicateOrInvalid_CountsSkippedRows()
    {
        string filePath = CreateTempCsv(
            "IdentifierName,DisplayName",
            "taken1,Already Taken",
            ",Missing Identifier",
            "invalid-name,Invalid Identifier",
            "good1,Good Player");

        Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock(existingIdentifiers: ["taken1"]);

        (List<Player> validPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences: null);

        Player player = Assert.Single(validPlayers);
        Assert.Equal(3, invalidCount);
        Assert.Equal("good1", player.IdentifierName);
        Assert.Equal("Good Player", player.DisplayName);
    }

    [Fact]
    public async Task ExportPlayersToCsv_OutputCanBeImportedBackSuccessfully()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            string filePath = CreateTempCsvPath();
            List<Player> exportedPlayers =
            [
                new Player
                {
                    IdentifierName = "alpha1",
                    DisplayName = "Alpha One",
                    LichessUsername = "alpha-lichess",
                    ChessComUsername = "alpha-chesscom",
                    Email = "alpha@example.com",
                    Phone = "555-0101",
                    FideId = "123456",
                    CbxId = "654321",
                    UscfId = "111222",
                    ChessTitle = ChessTitle.FM,
                    BirthDate = new DateTime(1990, 4, 20),
                    BulletRating = new ChessRating { Rating = 1800, Deviation = 45, Volatility = 0.06 },
                    BlitzRating = new ChessRating { Rating = 1810, Deviation = 46, Volatility = 0.06 },
                    RapidRating = new ChessRating { Rating = 1820, Deviation = 47, Volatility = 0.06 },
                    ClassicalRating = new ChessRating { Rating = 1830, Deviation = 48, Volatility = 0.06 }
                },
                new Player
                {
                    IdentifierName = "bravo2",
                    DisplayName = "Bravo Two",
                    ChessTitle = ChessTitle.None,
                    BirthDate = new DateTime(1985, 12, 1),
                    BulletRating = new ChessRating { Rating = 1500, Deviation = 60, Volatility = 0.07 },
                    BlitzRating = new ChessRating { Rating = 1510, Deviation = 61, Volatility = 0.07 },
                    RapidRating = new ChessRating { Rating = 1520, Deviation = 62, Volatility = 0.07 },
                    ClassicalRating = new ChessRating { Rating = 1530, Deviation = 63, Volatility = 0.07 }
                }
            ];

            await CsvImportExportHelper.ExportPlayersToCsv(filePath, exportedPlayers);

            Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock();
            ClubPreferences clubPreferences = new ClubPreferences
            {
                Deviation = 72,
                Volatility = 0.09,
            };

            (List<Player> importedPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences);

            Assert.Equal(0, invalidCount);
            Assert.Equal(2, importedPlayers.Count);

            AssertPlayersMatchAfterRoundTrip(exportedPlayers[0], importedPlayers[0]);
            AssertPlayersMatchAfterRoundTrip(exportedPlayers[1], importedPlayers[1]);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task ImportPlayersFromCsv_WithPerColumnDeviation_UsesProvidedDeviationInsteadOfFallback()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            string filePath = CreateTempCsv(
                "IdentifierName,DisplayName,BulletRating,BulletDeviation",
                "alpha1,Alpha One,1800,25");

            Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock();
            ClubPreferences clubPreferences = new ClubPreferences
            {
                Deviation = 72,
                Volatility = 0.09,
            };

            (List<Player> validPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences);

            Player player = Assert.Single(validPlayers);
            Assert.Equal(0, invalidCount);
            Assert.Equal(1800, player.BulletRating.Rating);
            Assert.Equal(25, player.BulletRating.Deviation);
            Assert.Equal(0.09, player.BulletRating.Volatility);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task ImportPlayersFromCsv_WhenRatingExceedsClubBounds_ClampsToClubLimits()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            string filePath = CreateTempCsv(
                "IdentifierName,DisplayName,BulletRating,BlitzRating",
                "alpha1,Alpha One,50,5000");

            Mock<IPlayerService> playerServiceMock = CreatePlayerServiceMock();
            ClubPreferences clubPreferences = new ClubPreferences
            {
                MinRating = 120,
                MaxRating = 3600,
                Deviation = 72,
                Volatility = 0.09,
            };

            (List<Player> validPlayers, int invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, playerServiceMock.Object, clubPreferences);

            Player player = Assert.Single(validPlayers);
            Assert.Equal(0, invalidCount);
            Assert.Equal(120, player.BulletRating.Rating);
            Assert.Equal(3600, player.BlitzRating.Rating);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static Mock<IPlayerService> CreatePlayerServiceMock(IEnumerable<string>? existingIdentifiers = null)
    {
        HashSet<string> existingIdentifierSet = new(existingIdentifiers ?? [], StringComparer.OrdinalIgnoreCase);
        Mock<IPlayerService> playerServiceMock = new();

        playerServiceMock
            .Setup(service => service.PlayerIdentifierExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((string identifier, string? _) => existingIdentifierSet.Contains(identifier));

        return playerServiceMock;
    }

    private string CreateTempCsv(params string[] lines)
    {
        string filePath = CreateTempCsvPath();
        File.WriteAllLines(filePath, lines);
        return filePath;
    }

    private string CreateTempCsvPath()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        _tempFiles.Add(filePath);
        return filePath;
    }

    private static void AssertPlayersMatchAfterRoundTrip(Player expected, Player actual)
    {
        Assert.Equal(expected.IdentifierName, actual.IdentifierName);
        Assert.Equal(expected.DisplayName, actual.DisplayName);
        Assert.Equal(expected.LichessUsername, actual.LichessUsername);
        Assert.Equal(expected.ChessComUsername, actual.ChessComUsername);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.Phone, actual.Phone);
        Assert.Equal(expected.FideId, actual.FideId);
        Assert.Equal(expected.CbxId, actual.CbxId);
        Assert.Equal(expected.UscfId, actual.UscfId);
        Assert.Equal(expected.ChessTitle, actual.ChessTitle);
        Assert.Equal(expected.BirthDate, actual.BirthDate);
        Assert.Equal(expected.BulletRating.Rating, actual.BulletRating.Rating);
        Assert.Equal(expected.BulletRating.Deviation, actual.BulletRating.Deviation);
        Assert.Equal(expected.BlitzRating.Rating, actual.BlitzRating.Rating);
        Assert.Equal(expected.BlitzRating.Deviation, actual.BlitzRating.Deviation);
        Assert.Equal(expected.RapidRating.Rating, actual.RapidRating.Rating);
        Assert.Equal(expected.RapidRating.Deviation, actual.RapidRating.Deviation);
        Assert.Equal(expected.ClassicalRating.Rating, actual.ClassicalRating.Rating);
        Assert.Equal(expected.ClassicalRating.Deviation, actual.ClassicalRating.Deviation);
    }
}
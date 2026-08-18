using System.ComponentModel;
using System.Reflection;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Tests.TestUtils;
using Microsoft.Extensions.Time.Testing;

namespace LocalKnights.Common.Tests.Services.Database;

public class PlayerServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly ClubService _clubService;
    private readonly PlayerService _playerService;
    private readonly FakeTimeProvider _timeProvider;

    public PlayerServiceTests()
    {
        _timeProvider = new FakeTimeProvider();
        _databaseService = new DatabaseService(MockHelper.CreateMockFileSystemService(), MockHelper.CreateMockLogger());
        _databaseService.OpenInMemoryDbConnection();

        _clubService = new ClubService(_databaseService, MockHelper.CreateMockFileSystemService(), MockHelper.CreateMockLogger());
        SetCurrentClub(_clubService, new ClubPreferences
        {
            UpdatedAt = _timeProvider.GetUtcNow().DateTime,
            IdentifierName = "test-club",
            DisplayName = "Test Club",
            RatingSystem = RatingSystem.Glicko2,
            Rating = 1500,
            Deviation = 350,
            Volatility = 0.06,
            ProvisionalGames = 20,
        });

        _playerService = new PlayerService(_databaseService, _clubService, _timeProvider);

        SetupPlayersAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _databaseService.CloseConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreatePlayerAsync_SetsCreatedAt_AndPersistsPlayer()
    {
        DateTime expectedCreatedAt = _timeProvider.GetUtcNow().DateTime;
        Player player = new()
        {
            IdentifierName = "createdplayer",
            DisplayName = "Created Player",
        };

        Player createdPlayer = await _playerService.CreatePlayerAsync(player);
        Player? persistedPlayer = await _playerService.GetPlayerByIdentifierAsync("createdplayer");

        Assert.Equal(expectedCreatedAt, createdPlayer.CreatedAt);
        Assert.NotNull(persistedPlayer);
        Assert.Equal("Created Player", persistedPlayer!.DisplayName);
        Assert.Equal(expectedCreatedAt, persistedPlayer.CreatedAt);
    }

    [Fact]
    public async Task CreatePlayerAsync_WhenRatingsAreMissing_UsesClubDefaultRatings()
    {
        Player player = new()
        {
            IdentifierName = "defaultratings",
            DisplayName = "Default Ratings",
        };

        Player createdPlayer = await _playerService.CreatePlayerAsync(player);

        Assert.Equal(_clubService.CurrentClub.Rating, createdPlayer.BulletRating.Rating);
        Assert.Equal(_clubService.CurrentClub.Deviation, createdPlayer.BulletRating.Deviation);
        Assert.Equal(_clubService.CurrentClub.Volatility, createdPlayer.BulletRating.Volatility);
        Assert.Equal(_clubService.CurrentClub.Rating, createdPlayer.BlitzRating.Rating);
        Assert.Equal(_clubService.CurrentClub.Rating, createdPlayer.RapidRating.Rating);
        Assert.Equal(_clubService.CurrentClub.Rating, createdPlayer.ClassicalRating.Rating);
    }

    [Fact]
    public async Task CreatePlayerAsync_WhenRatingsAreProvided_PersistsClonedRatings()
    {
        ChessRating originalBulletRating = new ChessRating { Rating = 1200, Deviation = 100, Volatility = 0.04 };
        ChessRating originalBlitzRating = new ChessRating { Rating = 1300, Deviation = 110, Volatility = 0.05 };
        ChessRating originalRapidRating = new ChessRating { Rating = 1400, Deviation = 120, Volatility = 0.06 };
        ChessRating originalClassicalRating = new ChessRating { Rating = 1500, Deviation = 130, Volatility = 0.07 };

        Player player = new()
        {
            IdentifierName = "providedratings",
            DisplayName = "Provided Ratings",
            BulletRating = originalBulletRating,
            BlitzRating = originalBlitzRating,
            RapidRating = originalRapidRating,
            ClassicalRating = originalClassicalRating,
        };

        Player createdPlayer = await _playerService.CreatePlayerAsync(player);
        Player? persistedPlayer = await _playerService.GetPlayerByIdentifierAsync("providedratings");

        Assert.NotSame(originalBulletRating, createdPlayer.BulletRating);
        Assert.NotSame(originalBlitzRating, createdPlayer.BlitzRating);
        Assert.NotSame(originalRapidRating, createdPlayer.RapidRating);
        Assert.NotSame(originalClassicalRating, createdPlayer.ClassicalRating);
        Assert.NotNull(persistedPlayer);
        Assert.Equal(1200, persistedPlayer!.BulletRating.Rating);
        Assert.Equal(1300, persistedPlayer.BlitzRating.Rating);
        Assert.Equal(1400, persistedPlayer.RapidRating.Rating);
        Assert.Equal(1500, persistedPlayer.ClassicalRating.Rating);
    }

    [Fact]
    public async Task GetPlayerByIdentifierAsync_WhenIdentifierExists_ReturnsPlayer()
    {
        Player? player = await _playerService.GetPlayerByIdentifierAsync("alpha");

        Assert.NotNull(player);
        Assert.Equal("Alpha Player", player!.DisplayName);
    }

    [Fact]
    public async Task GetPlayerByIdentifierAsync_WhenIdentifierDoesNotExist_ReturnsNull()
    {
        Player? player = await _playerService.GetPlayerByIdentifierAsync("missing");

        Assert.Null(player);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPlayerByIdentifierAsync_WhenIdentifierIsBlank_ReturnsNull(string identifier)
    {
        Player? player = await _playerService.GetPlayerByIdentifierAsync(identifier);

        Assert.Null(player);
    }

    [Fact]
    public async Task PlayerIdentifierExistsAsync_WhenIdentifierExists_ReturnsTrue()
    {
        bool exists = await _playerService.PlayerIdentifierExistsAsync("alpha");

        Assert.True(exists);
    }

    [Fact]
    public async Task PlayerIdentifierExistsAsync_WhenIdentifierDoesNotExist_ReturnsFalse()
    {
        bool exists = await _playerService.PlayerIdentifierExistsAsync("nope");

        Assert.False(exists);
    }

    [Fact]
    public async Task PlayerIdentifierExistsAsync_WhenExcludedIdentifierMatchesSamePlayer_ReturnsFalse()
    {
        bool exists = await _playerService.PlayerIdentifierExistsAsync("alpha", "alpha");

        Assert.False(exists);
    }

    [Fact]
    public async Task UpdatePlayerAsync_PersistsScalarFieldChanges()
    {
        Player player = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        player.DisplayName = "Updated Alpha";
        player.Email = "alpha@example.com";
        player.Phone = "123456";

        Player? updatedPlayer = await _playerService.UpdatePlayerAsync(player);
        Player? persistedPlayer = await _playerService.GetPlayerByIdentifierAsync("alpha");

        Assert.NotNull(updatedPlayer);
        Assert.NotNull(persistedPlayer);
        Assert.Equal("Updated Alpha", persistedPlayer!.DisplayName);
        Assert.Equal("alpha@example.com", persistedPlayer.Email);
        Assert.Equal("123456", persistedPlayer.Phone);
    }

    [Fact]
    public async Task UpdatePlayerAsync_PersistsRatingChanges()
    {
        Player player = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        player.BlitzRating = new ChessRating { Rating = 1888, Deviation = 77, Volatility = 0.03 };

        await _playerService.UpdatePlayerAsync(player);

        Player persistedPlayer = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;

        Assert.Equal(1888, persistedPlayer.BlitzRating.Rating);
        Assert.Equal(77, persistedPlayer.BlitzRating.Deviation);
        Assert.Equal(0.03, persistedPlayer.BlitzRating.Volatility);
    }

    [Fact]
    public async Task DeletePlayerAsync_WhenPlayerExists_DeletesPlayerAndReturnsTrue()
    {
        bool deleted = await _playerService.DeletePlayerAsync("alpha");
        Player? player = await _playerService.GetPlayerByIdentifierAsync("alpha");

        Assert.True(deleted);
        Assert.Null(player);
    }

    [Fact]
    public async Task DeletePlayerAsync_WhenPlayerDoesNotExist_ReturnsFalse()
    {
        bool deleted = await _playerService.DeletePlayerAsync("missing");

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllPlayersAsync_ReturnsPlayersOrderedByIdentifierName()
    {
        List<Player> players = await _playerService.GetAllPlayersAsync();

        Assert.Equal(players.OrderBy(p => p.IdentifierName).Select(p => p.IdentifierName).ToArray(), players.Select(p => p.IdentifierName).ToArray());
    }

    [Fact]
    public async Task GetPlayersPagedAsync_WithoutSearch_ReturnsRequestedPageInRequestedSortOrder()
    {
        List<Player> page = await _playerService.GetPlayersPagedAsync(1, 2, new SortParameter(nameof(Player.DisplayName), ListSortDirection.Descending));

        Assert.Equal(2, page.Count);
        Assert.Equal(page.OrderByDescending(p => p.DisplayName).Select(p => p.IdentifierName).ToArray(), page.Select(p => p.IdentifierName).ToArray());
    }

    [Fact]
    public async Task GetPlayersPagedAsync_WithSearch_FiltersByIdentifierOrDisplayName()
    {
        List<Player> players = await _playerService.GetPlayersPagedAsync(1, 10, "alpha", new SortParameter(nameof(Player.IdentifierName), ListSortDirection.Ascending));

        Assert.Single(players);
        Assert.Equal("alpha", players[0].IdentifierName);
    }

    [Fact]
    public async Task SearchPlayersAsync_ReturnsSortedLimitedMatches()
    {
        List<Player> players = await _playerService.SearchPlayersAsync("player", maxResults: 2);

        Assert.Equal(2, players.Count);
        Assert.Equal(players.OrderBy(p => p.DisplayName).ThenBy(p => p.IdentifierName).Select(p => p.IdentifierName).ToArray(), players.Select(p => p.IdentifierName).ToArray());
    }

    [Fact]
    public async Task SearchPlayersAsync_ExcludesSpecifiedIdentifier()
    {
        List<Player> players = await _playerService.SearchPlayersAsync("player", excludedPlayerIdentifierName: "alpha", maxResults: 10);

        Assert.DoesNotContain(players, player => player.IdentifierName == "alpha");
    }

    [Fact]
    public async Task GetPlayersCountAsync_WithoutSearch_ReturnsTotalCount()
    {
        int count = await _playerService.GetPlayersCountAsync();

        Assert.Equal(4, count);
    }

    [Fact]
    public async Task GetPlayersCountAsync_WithSearch_ReturnsFilteredCount()
    {
        int count = await _playerService.GetPlayersCountAsync("player");

        Assert.Equal(4, count);
    }

    [Fact]
    public void GetPlayerTimeFormatPosition_OrdersByRatingThenDeviationThenCreatedAt()
    {
        int alphaPosition = _playerService.GetPlayerTimeFormatPosition("alpha", TimeFormat.Blitz);
        int betaPosition = _playerService.GetPlayerTimeFormatPosition("beta", TimeFormat.Blitz);
        int gammaPosition = _playerService.GetPlayerTimeFormatPosition("gamma", TimeFormat.Blitz);

        Assert.Equal(1, alphaPosition);
        Assert.Equal(2, betaPosition);
        Assert.Equal(3, gammaPosition);
    }

    [Fact]
    public void GetPlayerTimeFormatPosition_WhenPlayerDoesNotExist_ReturnsZero()
    {
        int position = _playerService.GetPlayerTimeFormatPosition("missing", TimeFormat.Blitz);

        Assert.Equal(0, position);
    }

    [Fact]
    public async Task GetPlayerRatedGamesCountSinceLastYear_WithTimeFormatFilter_ReturnsFilteredCount()
    {
        await _databaseService.AddAsync(new Game
        {
            WhitePlayerId = "alpha",
            BlackPlayerId = "beta",
            TimeFormat = TimeFormat.Blitz,
            IsRated = true,
            Result = GameResult.WhiteWins,
            CreatedAt = _timeProvider.GetUtcNow().DateTime,
        });

        await _databaseService.AddAsync(new Game
        {
            WhitePlayerId = "alpha",
            BlackPlayerId = "gamma",
            TimeFormat = TimeFormat.Rapid,
            IsRated = true,
            Result = GameResult.Draw,
            CreatedAt = _timeProvider.GetUtcNow().DateTime,
        });

        int blitzCount = await _playerService.GetPlayerRatedGamesCountSinceLastYear("alpha", TimeFormat.Blitz);
        int totalCount = await _playerService.GetPlayerRatedGamesCountSinceLastYear("alpha", null);

        Assert.Equal(1, blitzCount);
        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task GetPlayerRatedGamesCountSinceLastYear_ExcludesOldOrUnratedGames()
    {
        await _databaseService.AddAsync(new Game
        {
            WhitePlayerId = "alpha",
            BlackPlayerId = "beta",
            TimeFormat = TimeFormat.Blitz,
            IsRated = false,
            Result = GameResult.WhiteWins,
            CreatedAt = _timeProvider.GetUtcNow().DateTime,
        });

        await _databaseService.AddAsync(new Game
        {
            WhitePlayerId = "alpha",
            BlackPlayerId = "gamma",
            TimeFormat = TimeFormat.Blitz,
            IsRated = true,
            Result = GameResult.Draw,
            CreatedAt = _timeProvider.GetUtcNow().DateTime.AddYears(-2),
        });

        int count = await _playerService.GetPlayerRatedGamesCountSinceLastYear("alpha", null);

        Assert.Equal(0, count);
    }

    private async Task SetupPlayersAsync()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "alpha",
            DisplayName = "Alpha Player",
            BlitzRating = new ChessRating { Rating = 1800, Deviation = 40, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1700, Deviation = 50, Volatility = 0.06 },
            BulletRating = new ChessRating { Rating = 1600, Deviation = 60, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1750, Deviation = 55, Volatility = 0.06 },
        });

        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)));
        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "beta",
            DisplayName = "Beta Player",
            BlitzRating = new ChessRating { Rating = 1800, Deviation = 50, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1650, Deviation = 45, Volatility = 0.06 },
            BulletRating = new ChessRating { Rating = 1500, Deviation = 65, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1680, Deviation = 60, Volatility = 0.06 },
        });

        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)));
        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "gamma",
            DisplayName = "Gamma Player",
            BlitzRating = new ChessRating { Rating = 1750, Deviation = 30, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1600, Deviation = 40, Volatility = 0.06 },
            BulletRating = new ChessRating { Rating = 1450, Deviation = 50, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1660, Deviation = 55, Volatility = 0.06 },
        });

        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)));
        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "delta",
            DisplayName = "Delta Player",
            BlitzRating = new ChessRating { Rating = 1600, Deviation = 70, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1550, Deviation = 70, Volatility = 0.06 },
            BulletRating = new ChessRating { Rating = 1400, Deviation = 70, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1500, Deviation = 70, Volatility = 0.06 },
        });
    }

    private static void SetCurrentClub(ClubService clubService, ClubPreferences clubPreferences)
    {
        PropertyInfo? currentClubProperty = typeof(ClubService).GetProperty(
            nameof(ClubService.CurrentClub),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        currentClubProperty!.SetValue(clubService, clubPreferences);
    }
}
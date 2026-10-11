using System.ComponentModel;
using System.Reflection;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Tests.TestUtils;
using Microsoft.Extensions.Time.Testing;

namespace LocalKnights.Common.Tests.Services.Database;

public class GameServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly ClubService _clubService;
    private readonly PlayerService _playerService;
    private readonly GameService _gameService;
    private readonly FakeTimeProvider _timeProvider;

    public GameServiceTests()
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
        _gameService = new GameService(_databaseService, _clubService, _playerService, _timeProvider);

        SetupPlayersAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _databaseService.CloseConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GameExists_WhenGameExists_ReturnsTrue()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);

        bool exists = _gameService.GameExists(game.Id);

        Assert.True(exists);
    }

    [Fact]
    public void GameExists_WhenGameDoesNotExist_ReturnsFalse()
    {
        bool exists = _gameService.GameExists(99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetAllGamesAsync_ReturnsGamesOrderedByCreatedAtThenIdDescending()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)));
        await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);
        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc)));
        Game latest = await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Rapid, GameResult.WhiteWins, false);

        List<Game> games = await _gameService.GetAllGamesAsync();

        Assert.Equal(latest.Id, games.First().Id);
        Assert.Equal(games.OrderByDescending(g => g.CreatedAt).ThenByDescending(g => g.Id).Select(g => g.Id).ToArray(), games.Select(g => g.Id).ToArray());
    }

    [Fact]
    public async Task GetGameByIdAsync_WhenGameExists_ReturnsGame()
    {
        Game createdGame = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, false);

        Game? game = await _gameService.GetGameByIdAsync(createdGame.Id);

        Assert.NotNull(game);
        Assert.Equal(createdGame.Id, game!.Id);
        Assert.Equal("alpha", game.WhitePlayerId);
        Assert.Equal("beta", game.BlackPlayerId);
    }

    [Fact]
    public async Task GetGameByIdAsync_WhenGameDoesNotExist_ReturnsNull()
    {
        Game? game = await _gameService.GetGameByIdAsync(99999);

        Assert.Null(game);
    }

    [Fact]
    public async Task GetAllPlayersAsync_ReturnsPlayersOrderedByDisplayNameThenIdentifier()
    {
        List<Player> players = await _gameService.GetAllPlayersAsync();

        Assert.Equal(players.OrderBy(p => p.DisplayName).ThenBy(p => p.IdentifierName).Select(p => p.IdentifierName).ToArray(), players.Select(p => p.IdentifierName).ToArray());
    }

    [Fact]
    public async Task CreateGameAsync_WhenPlayersAreEqual_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _gameService.CreateGameAsync("alpha", "alpha", TimeFormat.Blitz, GameResult.Draw, false, null));
    }

    [Fact]
    public async Task CreateGameAsync_UnratedGame_PersistsGameWithoutChangingPlayerRatings()
    {
        Player whiteBefore = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        Player blackBefore = (await _playerService.GetPlayerByIdentifierAsync("beta"))!;

        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, false, null);

        Player whiteAfter = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        Player blackAfter = (await _playerService.GetPlayerByIdentifierAsync("beta"))!;

        Assert.False(game.IsRated);
        Assert.Equal(whiteBefore.BlitzRating.Rating, whiteAfter.BlitzRating.Rating);
        Assert.Equal(blackBefore.BlitzRating.Rating, blackAfter.BlitzRating.Rating);
    }

    [Fact]
    public async Task CreateGameAsync_RatedGameWithExistingPlayers_PersistsGameAndUpdatesRatings()
    {
        Player whiteBefore = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        Player blackBefore = (await _playerService.GetPlayerByIdentifierAsync("beta"))!;

        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, true, null);

        Player whiteAfter = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        Player blackAfter = (await _playerService.GetPlayerByIdentifierAsync("beta"))!;

        Assert.True(game.IsRated);
        Assert.Equal(whiteBefore.BlitzRating.Rating, game.WhitePlayerRating.Rating);
        Assert.Equal(blackBefore.BlitzRating.Rating, game.BlackPlayerRating.Rating);
        Assert.NotEqual(whiteBefore.BlitzRating.Rating, whiteAfter.BlitzRating.Rating);
        Assert.NotEqual(blackBefore.BlitzRating.Rating, blackAfter.BlitzRating.Rating);
    }

    [Fact]
    public async Task CreateGameAsync_RatedGameWithMissingPlayers_PersistsGameWithoutUpdatingRatings()
    {
        Game game = await _gameService.CreateGameAsync("ghostwhite", "ghostblack", TimeFormat.Blitz, GameResult.Draw, true, null);

        Assert.True(game.IsRated);
        Assert.Equal(_clubService.CurrentClub.Rating, game.WhitePlayerRating.Rating);
        Assert.Equal(_clubService.CurrentClub.Rating, game.BlackPlayerRating.Rating);
        Assert.Null(await _playerService.GetPlayerByIdentifierAsync("ghostwhite"));
        Assert.Null(await _playerService.GetPlayerByIdentifierAsync("ghostblack"));
    }

    [Fact]
    public async Task CreateGameAsync_WithWhitespacePgn_PersistsNullPgn()
    {
        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false, "   ");

        Assert.Null(game.Pgn);
    }

    [Fact]
    public async Task CreateGameAsync_WithPgn_PersistsNormalizedPgn()
    {
        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false, "1. e4 e5");

        Assert.Equal("1. e4 e5", game.Pgn);
    }

    [Fact]
    public async Task CreateGameAsync_RatedEloGame_UsesRatedGamesCountForKFactorPath()
    {
        SetCurrentClub(_clubService, new ClubPreferences
        {
            UpdatedAt = _timeProvider.GetUtcNow().DateTime,
            IdentifierName = "test-club",
            DisplayName = "Test Club",
            RatingSystem = RatingSystem.ELO,
            Rating = 1500,
            Deviation = 350,
            Volatility = 0.06,
            ProvisionalGames = 20,
        });

        await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Blitz, GameResult.Draw, true, createdAt: _timeProvider.GetUtcNow().DateTime.AddMonths(-1));
        Player whiteBefore = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;

        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, true, null);
        Player whiteAfter = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;

        Assert.True(game.IsRated);
        Assert.NotEqual(whiteBefore.BlitzRating.Rating, whiteAfter.BlitzRating.Rating);
    }

    [Fact]
    public async Task UpdateGamePgnAsync_WhenGameExists_UpdatesPgnAndReturnsTrue()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);

        bool updated = await _gameService.UpdateGamePgnAsync(game.Id, "1. d4 d5");
        Game persistedGame = (await _gameService.GetGameByIdAsync(game.Id))!;

        Assert.True(updated);
        Assert.Equal("1. d4 d5", persistedGame.Pgn);
    }

    [Fact]
    public async Task UpdateGamePgnAsync_WhenPgnIsWhitespace_ClearsPgnAndReturnsTrue()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false, pgn: "1. e4 e5");

        bool updated = await _gameService.UpdateGamePgnAsync(game.Id, "   ");
        Game persistedGame = (await _gameService.GetGameByIdAsync(game.Id))!;

        Assert.True(updated);
        Assert.Null(persistedGame.Pgn);
    }

    [Fact]
    public async Task UpdateGamePgnAsync_WhenGameDoesNotExist_ReturnsFalse()
    {
        bool updated = await _gameService.UpdateGamePgnAsync(99999, "1. c4");

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameExists_DeletesRowAndReturnsTrue()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);

        bool deleted = await _gameService.DeleteGameAsync(game.Id);

        Assert.True(deleted);
        Assert.Null(await _gameService.GetGameByIdAsync(game.Id));
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameDoesNotExist_ReturnsFalse()
    {
        bool deleted = await _gameService.DeleteGameAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteGamesAsync_RemovesOnlyRequestedGames()
    {
        Game game1 = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);
        Game game2 = await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Rapid, GameResult.WhiteWins, false);
        Game game3 = await CreateGameRecordAsync("beta", "gamma", TimeFormat.Classical, GameResult.BlackWins, false);

        await _gameService.DeleteGamesAsync([game1.Id, game3.Id]);

        List<Game> games = await _gameService.GetAllGamesAsync();

        Assert.Single(games);
        Assert.Equal(game2.Id, games[0].Id);
    }

    [Fact]
    public async Task GetGamesPagedAsync_DefaultSort_ReturnsCreatedAtDescendingPage()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)));
        await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);
        _timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc)));
        Game latest = await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Rapid, GameResult.WhiteWins, false);

        List<Game> page = await _gameService.GetGamesPagedAsync(1, 1);

        Assert.Single(page);
        Assert.Equal(latest.Id, page[0].Id);
    }

    [Fact]
    public async Task GetGamesPagedAsync_SortByResultOrTimeFormat_AppliesRequestedOrdering()
    {
        await CreateGameRecordAsync("alpha", "beta", TimeFormat.Classical, GameResult.BlackWins, false);
        await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Blitz, GameResult.Draw, false);

        List<Game> page = await _gameService.GetGamesPagedAsync(1, 10, new SortParameter(nameof(Game.TimeFormat), ListSortDirection.Ascending));

        Assert.Equal(page.OrderBy(game => game.TimeFormat).Select(game => game.Id).ToArray(), page.Select(game => game.Id).ToArray());
    }

    [Fact]
    public async Task GetGamesCountAsync_ReturnsTotalPersistedGameCount()
    {
        await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);
        await CreateGameRecordAsync("alpha", "gamma", TimeFormat.Rapid, GameResult.WhiteWins, false);

        int count = await _gameService.GetGamesCountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetCompetitionIdForGameAsync_WhenPairingExists_ReturnsCompetitionId()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);
        Competition competition = new()
        {
            Title = "Game Competition",
            Type = CompetitionType.ChessMatch,
            State = CompetitionState.Ongoing,
            TimeFormat = TimeFormat.Blitz,
        };

        await _databaseService.AddAsync(competition);
        await _databaseService.AddAsync(new CompetitionPairing
        {
            CompetitionId = competition.Id,
            RoundNumber = 1,
            TableNumber = 1,
            WhitePlayerIdentifierName = "alpha",
            BlackPlayerIdentifierName = "beta",
            GameId = game.Id,
        });

        int? competitionId = await _gameService.GetCompetitionIdForGameAsync(game.Id);

        Assert.Equal(competition.Id, competitionId);
    }

    [Fact]
    public async Task GetCompetitionIdForGameAsync_WhenNoPairingExists_ReturnsNull()
    {
        Game game = await CreateGameRecordAsync("alpha", "beta", TimeFormat.Blitz, GameResult.Draw, false);

        int? competitionId = await _gameService.GetCompetitionIdForGameAsync(game.Id);

        Assert.Null(competitionId);
    }

    [Fact]
    public async Task CreateGameAsync_PersistsOldRatingsAndRatingChangesOnGame()
    {
        Player whiteBefore = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;
        Player blackBefore = (await _playerService.GetPlayerByIdentifierAsync("beta"))!;

        Game game = await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, true, null);

        Assert.Equal(whiteBefore.BlitzRating.Rating, game.WhitePlayerRating.Rating);
        Assert.Equal(blackBefore.BlitzRating.Rating, game.BlackPlayerRating.Rating);
        Assert.NotEqual(0, game.WhitePlayerRatingChange);
        Assert.NotEqual(0, game.BlackPlayerRatingChange);
    }

    [Fact]
    public async Task CreateGameAsync_WhenClubUsesGlicko2_UpdatesRatingsWithoutUsingEloProvisionalLogic()
    {
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

        Player whiteBefore = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;

        await _gameService.CreateGameAsync("alpha", "beta", TimeFormat.Blitz, GameResult.WhiteWins, true, null);
        Player whiteAfter = (await _playerService.GetPlayerByIdentifierAsync("alpha"))!;

        Assert.NotEqual(whiteBefore.BlitzRating.Rating, whiteAfter.BlitzRating.Rating);
    }

    private async Task SetupPlayersAsync()
    {
        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "alpha",
            DisplayName = "Alpha Player",
            BulletRating = new ChessRating { Rating = 1600, Deviation = 60, Volatility = 0.06 },
            BlitzRating = new ChessRating { Rating = 1700, Deviation = 50, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1750, Deviation = 45, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1800, Deviation = 40, Volatility = 0.06 },
        });

        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "beta",
            DisplayName = "Beta Player",
            BulletRating = new ChessRating { Rating = 1500, Deviation = 70, Volatility = 0.06 },
            BlitzRating = new ChessRating { Rating = 1650, Deviation = 60, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1680, Deviation = 55, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1720, Deviation = 50, Volatility = 0.06 },
        });

        await _playerService.CreatePlayerAsync(new Player
        {
            IdentifierName = "gamma",
            DisplayName = "Gamma Player",
            BulletRating = new ChessRating { Rating = 1550, Deviation = 65, Volatility = 0.06 },
            BlitzRating = new ChessRating { Rating = 1620, Deviation = 58, Volatility = 0.06 },
            RapidRating = new ChessRating { Rating = 1660, Deviation = 53, Volatility = 0.06 },
            ClassicalRating = new ChessRating { Rating = 1700, Deviation = 48, Volatility = 0.06 },
        });
    }

    private async Task<Game> CreateGameRecordAsync(
        string whitePlayerId,
        string blackPlayerId,
        TimeFormat timeFormat,
        GameResult result,
        bool isRated,
        string? pgn = null,
        DateTime? createdAt = null)
    {
        Game game = await _gameService.CreateGameAsync(whitePlayerId, blackPlayerId, timeFormat, result, isRated, pgn);

        if (createdAt.HasValue)
        {
            game.CreatedAt = createdAt.Value;
            await _databaseService.UpdateAsync(game);
            game = (await _gameService.GetGameByIdAsync(game.Id))!;
        }

        return game;
    }

    private static void SetCurrentClub(ClubService clubService, ClubPreferences clubPreferences)
    {
        PropertyInfo? currentClubProperty = typeof(ClubService).GetProperty(
            nameof(ClubService.CurrentClub),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        currentClubProperty!.SetValue(clubService, clubPreferences);
    }
}
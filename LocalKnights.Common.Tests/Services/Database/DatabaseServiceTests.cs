using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Tests.TestUtils;
using Microsoft.Extensions.Time.Testing;
using System.Reflection;

namespace LocalKnights.Common.Tests.Services.Database;

public class DatabaseServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly ClubService _clubService;
    private readonly PlayerService _playerService;
    private readonly GameService _gameService;
    private readonly CompetitionService _competitionService;
    private readonly TimeProvider _timeProvider;

    // Will run before each [Fact] test
    public DatabaseServiceTests()
    {
        _timeProvider = new FakeTimeProvider();
        _databaseService = new DatabaseService(MockHelper.CreateMockFileSystemService(), MockHelper.CreateMockLogger());
        _databaseService.OpenInMemoryDbConnection();

        _clubService = new ClubService(_databaseService, MockHelper.CreateMockFileSystemService(), MockHelper.CreateMockLogger());

        _playerService = new PlayerService(_databaseService, _clubService, _timeProvider);
        _gameService = new GameService(_databaseService, _clubService, _playerService, _timeProvider);
        _competitionService = new CompetitionService(
            logger: MockHelper.CreateMockLogger(),
            databaseService: _databaseService,
            clubService: _clubService,
            playerService: _playerService,
            gameService: _gameService,
            randomService: MockHelper.CreateMockRandomService(),
            timeProvider: _timeProvider);

        SetupScenarioInDb().GetAwaiter().GetResult();
    }

    // Will run after each [Fact] test
    public void Dispose()
    {
        _databaseService.CloseConnectionAsync().GetAwaiter().GetResult();
    }

    private async Task SetupScenarioInDb()
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

        Player whitePlayer = new()
        {
            IdentifierName = "white-player",
            DisplayName = "White Player",
        };

        Player blackPlayer = new()
        {
            IdentifierName = "black-player",
            DisplayName = "Black Player",
        };

        Game game = new()
        {
            Id = 1,
            WhitePlayerId = whitePlayer.IdentifierName,
            BlackPlayerId = blackPlayer.IdentifierName,
            TimeFormat = TimeFormat.Rapid,
            Result = GameResult.WhiteWins,
        };

        Competition competition = new()
        {
            Id = 1,
            Title = "Finished Swiss",
            Type = CompetitionType.SwissSystemTournament,
            State = CompetitionState.Completed,
            TimeFormat = TimeFormat.Rapid,
            WinnerId = whitePlayer.IdentifierName,
        };

        CompetitionParticipant whiteParticipant = new()
        {
            Competition = competition,
            ParticipantIdentifierName = whitePlayer.IdentifierName,
            OldRating = new ChessRating(),
        };

        CompetitionParticipant blackParticipant = new()
        {
            Competition = competition,
            ParticipantIdentifierName = blackPlayer.IdentifierName,
            OldRating = new ChessRating(),
        };

        CompetitionPairing pairing = new()
        {
            Competition = competition,
            RoundNumber = 1,
            TableNumber = 1,
            WhitePlayerIdentifierName = whitePlayer.IdentifierName,
            BlackPlayerIdentifierName = blackPlayer.IdentifierName,
            GameId = game.Id,
        };

        await _databaseService.AddAsync(whitePlayer);
        await _databaseService.AddAsync(blackPlayer);
        await _databaseService.AddAsync(game);
        await _databaseService.AddAsync(competition);
        await _databaseService.AddAsync(whiteParticipant);
        await _databaseService.AddAsync(blackParticipant);
        await _databaseService.AddAsync(pairing);
    }

    [Fact]
    public async Task DeletingPlayers_KeepsGameIntact()
    {
        var players = await _databaseService.GetAllAsync<Player>();

        await _databaseService.DeleteRangeAsync(players);

        var games = await _databaseService.GetAllAsync<Game>();
        var serviceGames = await _gameService.GetAllGamesAsync();
        players = await _databaseService.GetAllAsync<Player>();
        Player? serviceWhitePlayer = await _playerService.GetPlayerByIdentifierAsync("white-player");
        Player? serviceBlackPlayer = await _playerService.GetPlayerByIdentifierAsync("black-player");

        Assert.Empty(players);
        Assert.Single(games);
        Assert.Single(serviceGames);
        Assert.Null(serviceWhitePlayer);
        Assert.Null(serviceBlackPlayer);
        Assert.Equal("white-player", games.First().WhitePlayerId);
        Assert.Equal("black-player", games.First().BlackPlayerId);
        Assert.Equal("white-player", serviceGames.First().WhitePlayerId);
        Assert.Equal("black-player", serviceGames.First().BlackPlayerId);
    }

    [Fact]
    public async Task DeletingPlayerThroughPlayerService_KeepsGamesCompetitionsParticipantsAndPairings_AndPreservesWinnerId()
    {
        bool deleted = await _playerService.DeletePlayerAsync("white-player");

        var players = await _databaseService.GetAllAsync<Player>();
        var games = await _databaseService.GetAllAsync<Game>();
        var competitions = await _databaseService.GetAllAsync<Competition>();
        var participants = await _databaseService.GetAllAsync<CompetitionParticipant>();
        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        List<Game> serviceGames = await _gameService.GetAllGamesAsync();
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.True(deleted);
        Assert.Single(players);
        Assert.DoesNotContain(players, player => player.IdentifierName == "white-player");
        Assert.Single(games);
        Assert.Single(competitions);
        Assert.Equal(2, participants.Count);
        Assert.Single(pairings);
        Assert.Single(serviceGames);
        Assert.Single(serviceCompetitions);
        Assert.NotNull(competition);

        Assert.Equal("white-player", games.First().WhitePlayerId);
        Assert.Equal("black-player", games.First().BlackPlayerId);
        Assert.Equal("white-player", participants.First(p => p.ParticipantIdentifierName == "white-player").ParticipantIdentifierName);
        Assert.Equal("black-player", participants.First(p => p.ParticipantIdentifierName == "black-player").ParticipantIdentifierName);
        Assert.Equal("white-player", pairings.First().WhitePlayerIdentifierName);
        Assert.Equal("black-player", pairings.First().BlackPlayerIdentifierName);
        Assert.Equal("white-player", competitions.First().WinnerId);
        Assert.Equal("white-player", serviceGames.First().WhitePlayerId);
        Assert.Equal("black-player", serviceGames.First().BlackPlayerId);
        Assert.Equal("white-player", serviceCompetitions.First().WinnerId);
        Assert.Equal(2, competition!.CompetitionParticipants.Count);
        Assert.Single(competition.CompetitionPairings);
    }

    [Fact]
    public async Task DeletingGame_KeepsCompetitionPairing_AndClearsGameReference()
    {
        var games = await _databaseService.GetAllAsync<Game>();

        await _databaseService.DeleteRangeAsync(games);

        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        games = await _databaseService.GetAllAsync<Game>();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.Empty(games);
        Assert.Single(pairings);
        Assert.Equal(1, pairings.First().GameId);
        Assert.NotNull(competition);
        Assert.Single(competition!.CompetitionPairings);
        Assert.Equal(1, competition.CompetitionPairings.First().GameId);
    }

    [Fact]
    public async Task DeletingGameThroughGameService_KeepsCompetitionPairing_AndClearsGameReference()
    {
        bool deleted = await _gameService.DeleteGameAsync(1);

        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        var games = await _databaseService.GetAllAsync<Game>();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.True(deleted);
        Assert.Empty(games);
        Assert.Single(pairings);
        Assert.NotNull(competition);
        Assert.Equal(1, pairings.First().GameId);
        Assert.Single(competition!.CompetitionPairings);
        Assert.Equal(1, competition.CompetitionPairings.First().GameId);
    }

    [Fact]
    public async Task DeletingCompetitionPairing_KeepsAssociatedGame()
    {
        var pairings = await _databaseService.GetWhereAsync<CompetitionPairing>(cp => cp.RoundNumber == 1 && cp.TableNumber == 1);

        await _databaseService.DeleteAsync(pairings.First());

        var games = await _databaseService.GetAllAsync<Game>();
        pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);
        Game? game = await _gameService.GetGameByIdAsync(1);

        Assert.Single(games);
        Assert.Empty(pairings);
        Assert.NotNull(game);
        Assert.NotNull(competition);
        Assert.Empty(competition!.CompetitionPairings);
    }

    [Fact]
    public async Task QueryingCompetition_HydratesWinnerFromWinnerId_WhenWinnerPlayerExists()
    {
        var competitions = await _databaseService.GetAllAsync<Competition>();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();

        Assert.Single(competitions);
        Assert.Equal("white-player", competitions.First().WinnerId);
        Assert.NotNull(competition);
        Assert.Single(serviceCompetitions);
    }

    [Fact]
    public async Task DeletingWinnerPlayer_KeepsWinnerId_AndClearsWinnerReference()
    {
        Player? winner = await _databaseService.GetByIdAsync<Player>("white-player");

        Assert.NotNull(winner);

        await _databaseService.DeleteAsync(winner!);

        var competitions = await _databaseService.GetAllAsync<Competition>();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();

        Assert.Single(competitions);
        Assert.Equal("white-player", competitions.First().WinnerId);
        Assert.NotNull(competition);
        Assert.Equal("white-player", competition!.WinnerId);
        Assert.Single(serviceCompetitions);
        Assert.Equal("white-player", serviceCompetitions.First().WinnerId);
    }

    [Fact]
    public async Task DeletingPlayer_KeepsGamesCompetitionsParticipantsAndPairings_AndPreservesWinnerId()
    {
        var players = await _databaseService.GetAllAsync<Player>();

        await _databaseService.DeleteRangeAsync(players);

        players = await _databaseService.GetAllAsync<Player>();
        var games = await _databaseService.GetAllAsync<Game>();
        var competitions = await _databaseService.GetAllAsync<Competition>();
        var participants = await _databaseService.GetAllAsync<CompetitionParticipant>();
        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        List<Game> serviceGames = await _gameService.GetAllGamesAsync();
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.Empty(players);
        Assert.Single(games);
        Assert.Single(competitions);
        Assert.Equal(2, participants.Count);
        Assert.Single(pairings);
        Assert.Single(serviceGames);
        Assert.Single(serviceCompetitions);
        Assert.NotNull(competition);

        Assert.Equal("white-player", games.First().WhitePlayerId);
        Assert.Equal("black-player", games.First().BlackPlayerId);
        Assert.Equal("white-player", participants.First(p => p.ParticipantIdentifierName == "white-player").ParticipantIdentifierName);
        Assert.Equal("black-player", participants.First(p => p.ParticipantIdentifierName == "black-player").ParticipantIdentifierName);
        Assert.Equal("white-player", pairings.First().WhitePlayerIdentifierName);
        Assert.Equal("black-player", pairings.First().BlackPlayerIdentifierName);
        Assert.Equal("white-player", competitions.First().WinnerId);
        Assert.Equal("white-player", serviceGames.First().WhitePlayerId);
        Assert.Equal("black-player", serviceGames.First().BlackPlayerId);
        Assert.Equal("white-player", serviceCompetitions.First().WinnerId);
        Assert.Equal(2, competition!.CompetitionParticipants.Count);
        Assert.Single(competition.CompetitionPairings);
    }

    [Fact]
    public async Task DeletingCompetition_KeepsGamesAndPlayers_RemovesPairingsAndParticipants()
    {
        var competitions = await _databaseService.GetAllAsync<Competition>();

        await _databaseService.DeleteRangeAsync(competitions);

        competitions = await _databaseService.GetAllAsync<Competition>();
        var games = await _databaseService.GetAllAsync<Game>();
        var players = await _databaseService.GetAllAsync<Player>();
        var participants = await _databaseService.GetAllAsync<CompetitionParticipant>();
        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        List<Game> serviceGames = await _gameService.GetAllGamesAsync();
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();

        Assert.Empty(competitions);
        Assert.Single(games);
        Assert.Equal(2, players.Count);
        Assert.Empty(participants);
        Assert.Empty(pairings);
        Assert.Single(serviceGames);
        Assert.Empty(serviceCompetitions);
    }

    [Fact]
    public async Task DeletingCompetitionThroughCompetitionService_KeepsGamesAndPlayers_RemovesPairingsAndParticipants()
    {
        bool deleted = await _competitionService.DeleteCompetitionAsync(1);

        var competitions = await _databaseService.GetAllAsync<Competition>();
        var games = await _databaseService.GetAllAsync<Game>();
        var players = await _databaseService.GetAllAsync<Player>();
        var participants = await _databaseService.GetAllAsync<CompetitionParticipant>();
        var pairings = await _databaseService.GetAllAsync<CompetitionPairing>();
        List<Game> serviceGames = await _gameService.GetAllGamesAsync();
        List<Competition> serviceCompetitions = await _competitionService.GetAllCompetitionsAsync();

        Assert.True(deleted);
        Assert.Empty(competitions);
        Assert.Single(games);
        Assert.Equal(2, players.Count);
        Assert.Empty(participants);
        Assert.Empty(pairings);
        Assert.Single(serviceGames);
        Assert.Empty(serviceCompetitions);
    }

    [Fact]
    public async Task UpdateAsync_PlayerDetachedEntity_PersistsScalarChanges()
    {
        Player player = (await _databaseService.GetAllAsync<Player>()).Single(p => p.IdentifierName == "white-player");

        player.DisplayName = "Updated White Player";
        player.Email = "white@example.com";

        await _databaseService.UpdateAsync(player);

        Player? reloadedPlayer = await _databaseService.GetByIdAsync<Player>("white-player");
        List<Player> allPlayers = await _databaseService.GetAllAsync<Player>();

        Assert.NotNull(reloadedPlayer);
        Assert.Equal("Updated White Player", reloadedPlayer!.DisplayName);
        Assert.Equal("white@example.com", reloadedPlayer.Email);
        Assert.Equal(2, allPlayers.Count);
    }

    [Fact]
    public async Task UpdateAsync_PlayerOwnedRatings_PersistsOwnedTypeChanges()
    {
        Player player = (await _databaseService.GetAllAsync<Player>()).Single(p => p.IdentifierName == "white-player");

        player.BlitzRating = new ChessRating
        {
            Rating = 1725,
            Deviation = 45,
            Volatility = 0.04,
        };

        await _databaseService.UpdateAsync(player);

        Player? reloadedPlayer = await _databaseService.GetByIdAsync<Player>("white-player");

        Assert.NotNull(reloadedPlayer);
        Assert.Equal(1725, reloadedPlayer!.BlitzRating.Rating);
        Assert.Equal(45, reloadedPlayer.BlitzRating.Deviation);
        Assert.Equal(0.04, reloadedPlayer.BlitzRating.Volatility);
    }

    [Fact]
    public async Task UpdateAsync_GameDetachedEntity_PersistsPgnAndNotesWithoutChangingPlayerIds()
    {
        Game game = (await _databaseService.GetAllAsync<Game>()).Single();

        game.Pgn = "1. e4 e5 2. Nf3 Nc6";
        game.Notes = "Test game notes";

        await _databaseService.UpdateAsync(game);

        Game? reloadedGame = await _databaseService.GetByIdAsync<Game>(1);

        Assert.NotNull(reloadedGame);
        Assert.Equal("1. e4 e5 2. Nf3 Nc6", reloadedGame!.Pgn);
        Assert.Equal("Test game notes", reloadedGame.Notes);
        Assert.Equal("white-player", reloadedGame.WhitePlayerId);
        Assert.Equal("black-player", reloadedGame.BlackPlayerId);
    }

    [Fact]
    public async Task UpdateAsync_CompetitionParticipantCompositeKey_PersistsOwnedRatingChanges()
    {
        CompetitionParticipant participant = (await _databaseService.GetWhereAsync<CompetitionParticipant>(p => p.CompetitionId == 1 && p.ParticipantIdentifierName == "white-player")).Single();

        participant.NewRating = new ChessRating
        {
            Rating = 1810,
            Deviation = 60,
            Volatility = 0.05,
        };

        await _databaseService.UpdateAsync(participant);

        CompetitionParticipant reloadedParticipant = (await _databaseService.GetWhereAsync<CompetitionParticipant>(p => p.CompetitionId == 1 && p.ParticipantIdentifierName == "white-player")).Single();

        Assert.NotNull(reloadedParticipant.NewRating);
        Assert.Equal(1810, reloadedParticipant.NewRating!.Rating);
        Assert.Equal(60, reloadedParticipant.NewRating.Deviation);
        Assert.Equal(0.05, reloadedParticipant.NewRating.Volatility);
    }

    [Fact]
    public async Task UpdateAsync_CompetitionPairingCompositeKey_PersistsResultAndGameId()
    {
        CompetitionPairing pairing = (await _databaseService.GetWhereAsync<CompetitionPairing>(p => p.CompetitionId == 1 && p.RoundNumber == 1 && p.TableNumber == 1)).Single();

        pairing.Result = GameResult.Draw;
        pairing.GameId = 77;

        await _databaseService.UpdateAsync(pairing);

        CompetitionPairing reloadedPairing = (await _databaseService.GetWhereAsync<CompetitionPairing>(p => p.CompetitionId == 1 && p.RoundNumber == 1 && p.TableNumber == 1)).Single();

        Assert.Equal(GameResult.Draw, reloadedPairing.Result);
        Assert.Equal(77, reloadedPairing.GameId);
    }

    [Fact]
    public async Task UpdateAsync_CompetitionDetachedAggregate_PersistsRootChangesWithoutDeletingParticipantsOrPairings()
    {
        Competition competition = (await _competitionService.GetCompetitionByIdAsync(1))!;

        competition.Title = "Updated Competition Title";
        competition.NumberOfRounds = 4;
        competition.ByeValue = 0.5;

        await _databaseService.UpdateAsync(competition);

        Competition reloadedCompetition = (await _competitionService.GetCompetitionByIdAsync(1))!;
        List<CompetitionParticipant> participants = await _databaseService.GetWhereAsync<CompetitionParticipant>(p => p.CompetitionId == 1);
        List<CompetitionPairing> pairings = await _databaseService.GetWhereAsync<CompetitionPairing>(p => p.CompetitionId == 1);

        Assert.Equal("Updated Competition Title", reloadedCompetition.Title);
        Assert.Equal(4, reloadedCompetition.NumberOfRounds);
        Assert.Equal(0.5, reloadedCompetition.ByeValue);
        Assert.Equal(2, participants.Count);
        Assert.Single(pairings);
    }

    [Fact]
    public async Task GetByIdAsync_CompositeKeyEntity_ReturnsCorrectCompetitionPairing()
    {
        CompetitionPairing? pairing = await _databaseService.GetByIdAsync<CompetitionPairing>(1, 1, 1);
        CompetitionParticipant? participant = await _databaseService.GetByIdAsync<CompetitionParticipant>(1, "white-player");

        Assert.NotNull(pairing);
        Assert.Equal("white-player", pairing!.WhitePlayerIdentifierName);
        Assert.Equal("black-player", pairing.BlackPlayerIdentifierName);

        Assert.NotNull(participant);
        Assert.Equal("white-player", participant!.ParticipantIdentifierName);
    }

    [Fact]
    public async Task ExecuteSqlAsync_UpdateStatement_PersistsRawSqlChanges()
    {
        await _databaseService.ExecuteSqlAsync("UPDATE Players SET DisplayName = {0} WHERE IdentifierName = {1}", "Raw SQL White", "white-player");

        Player reloadedPlayer = (await _databaseService.GetWhereAsync<Player>(player => player.IdentifierName == "white-player")).Single();

        Assert.Equal("Raw SQL White", reloadedPlayer.DisplayName);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDisconnected_ReturnsZero()
    {
        await _databaseService.CloseConnectionAsync();

        int result = await _databaseService.SaveChangesAsync();

        Assert.Equal(0, result);
    }

    private static void SetCurrentClub(ClubService clubService, ClubPreferences clubPreferences)
    {
        PropertyInfo? currentClubProperty = typeof(ClubService).GetProperty(
            nameof(ClubService.CurrentClub),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        currentClubProperty!.SetValue(clubService, clubPreferences);
    }
}

using System.Reflection;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using Microsoft.EntityFrameworkCore;
using LocalKnights.Common.Tests.TestUtils;
using Microsoft.Extensions.Time.Testing;

namespace LocalKnights.Common.Tests.Services.Database;

public class CompetitionServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly ClubService _clubService;
    private readonly PlayerService _playerService;
    private readonly GameService _gameService;
    private readonly CompetitionService _competitionService;
    private readonly TimeProvider _timeProvider;

    public CompetitionServiceTests()
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

    public void Dispose()
    {
        _databaseService.CloseConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AddParticipantAsync_AddsParticipant_AndHydratesPlayer()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.NotNull(competition);
        Assert.Empty(competition!.CompetitionParticipants.Where(participant => participant.ParticipantIdentifierName == "new-player"));

        Competition updatedCompetition = await _competitionService.AddParticipantAsync(competition, "new-player");

        Competition? reloadedCompetition = await _competitionService.GetCompetitionByIdAsync(1);
        CompetitionParticipant? addedParticipant = reloadedCompetition!.CompetitionParticipants.FirstOrDefault(participant => participant.ParticipantIdentifierName == "new-player");

        Assert.NotNull(addedParticipant);
        Assert.Equal(3, updatedCompetition.CompetitionParticipants.Count);
        Assert.Equal(3, reloadedCompetition.CompetitionParticipants.Count);
        Assert.Equal("new-player", addedParticipant!.ParticipantIdentifierName);
    }

    [Fact]
    public async Task AddParticipantAsync_PersistsCompetitionParticipantRowInTable()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.NotNull(competition);

        await _competitionService.AddParticipantAsync(competition!, "new-player");

        List<CompetitionParticipant> rows = await LoadCompetitionParticipantRowsAsync(1);

        Assert.Equal(["black-player", "new-player", "white-player"], rows.Select(row => row.ParticipantIdentifierName).OrderBy(name => name).ToArray());
    }

    [Fact]
    public async Task AddParticipantAsync_CreateStartingCompetition_PersistsCompetitionParticipantRowInTable()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["table-a", "table-b"]);

        await _competitionService.AddParticipantAsync(competition, "table-c");

        List<CompetitionParticipant> rows = await LoadCompetitionParticipantRowsAsync(competition.Id);

        Assert.Equal(["table-a", "table-b", "table-c"], rows.Select(row => row.ParticipantIdentifierName).OrderBy(name => name).ToArray());
    }

    [Fact]
    public async Task AddParticipantAsync_WhenParticipantAlreadyExists_DoesNotCreateDuplicate()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.NotNull(competition);
        Assert.Equal(2, competition!.CompetitionParticipants.Count);

        Competition updatedCompetition = await _competitionService.AddParticipantAsync(competition, "white-player");
        Competition? reloadedCompetition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.Equal(2, updatedCompetition.CompetitionParticipants.Count);
        Assert.Equal(2, reloadedCompetition!.CompetitionParticipants.Count);
        Assert.Single(reloadedCompetition.CompetitionParticipants.Where(participant => participant.ParticipantIdentifierName == "white-player"));
    }

    [Fact]
    public async Task RemoveParticipantAsync_RemovesParticipant_AndKeepsCompetitionIntact()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.NotNull(competition);
        Assert.Equal(2, competition!.CompetitionParticipants.Count);

        Competition updatedCompetition = await _competitionService.RemoveParticipantAsync(competition, "white-player");

        Competition? reloadedCompetition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.Single(updatedCompetition.CompetitionParticipants);
        Assert.Single(reloadedCompetition!.CompetitionParticipants);
        Assert.DoesNotContain(reloadedCompetition.CompetitionParticipants, participant => participant.ParticipantIdentifierName == "white-player");
        Assert.Contains(reloadedCompetition.CompetitionParticipants, participant => participant.ParticipantIdentifierName == "black-player");
    }

    [Fact]
    public async Task RemoveParticipantAsync_WhenParticipantDoesNotExist_LeavesCompetitionUnchanged()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.NotNull(competition);
        Assert.Equal(2, competition!.CompetitionParticipants.Count);

        Competition updatedCompetition = await _competitionService.RemoveParticipantAsync(competition, "missing-player");
        Competition? reloadedCompetition = await _competitionService.GetCompetitionByIdAsync(1);

        Assert.Equal(2, updatedCompetition.CompetitionParticipants.Count);
        Assert.Equal(2, reloadedCompetition!.CompetitionParticipants.Count);
        Assert.Contains(reloadedCompetition.CompetitionParticipants, participant => participant.ParticipantIdentifierName == "white-player");
        Assert.Contains(reloadedCompetition.CompetitionParticipants, participant => participant.ParticipantIdentifierName == "black-player");
    }

    [Fact]
    public async Task StartCompetitionAsync_SwissCompetitionWithTooManyRounds_ClampsRoundsBeforeStarting()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 5, participantIdentifiers: ["p1", "p2", "p3", "p4"]);

        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        Assert.Equal(CompetitionState.Ongoing, startedCompetition.State);
        Assert.Equal(1, startedCompetition.CurrentRound);
        Assert.Equal(SwissPairingAlgorithm.CalculateMaxNumberOfRounds(4), startedCompetition.NumberOfRounds);
        Assert.Equal(2, startedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 1));
    }

    [Fact]
    public async Task StartCompetitionAsync_SwissCompetitionWithinLimit_PreservesConfiguredRounds()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["p1", "p2", "p3", "p4"]);

        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        Assert.Equal(CompetitionState.Ongoing, startedCompetition.State);
        Assert.Equal(1, startedCompetition.CurrentRound);
        Assert.Equal(3, startedCompetition.NumberOfRounds);
    }

    [Fact]
    public async Task StartCompetitionAsync_ReloadedCompetition_PersistsRoundOneWithoutDuplicateTrackingConflicts()
    {
        Competition createdCompetition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["p5", "p6", "p7", "p8"]);
        Competition reloadedCompetition = (await _competitionService.GetCompetitionByIdAsync(createdCompetition.Id))!;

        Competition startedCompetition = await _competitionService.StartCompetitionAsync(reloadedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(createdCompetition.Id))!;

        Assert.Equal(CompetitionState.Ongoing, startedCompetition.State);
        Assert.Equal(1, startedCompetition.CurrentRound);
        Assert.Equal(2, startedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 1));
        Assert.Equal(CompetitionState.Ongoing, persistedCompetition.State);
        Assert.Equal(1, persistedCompetition.CurrentRound);
        Assert.Equal(2, persistedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 1));
    }

    [Fact]
    public async Task UpdateCompetitionAsync_ReloadedSwissCompetitionAfterParticipantAdds_PreservesCompetitionParticipantRowsInTable()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["rows-a", "rows-b"]);

        competition = await _competitionService.AddParticipantAsync(competition, "rows-c");
        List<CompetitionParticipant> rowsAfterAdd = await LoadCompetitionParticipantRowsAsync(competition.Id);

        Competition reloadedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;
        reloadedCompetition.NumberOfRounds = 2;
        List<CompetitionParticipant> rowsBeforeUpdate = await LoadCompetitionParticipantRowsAsync(competition.Id);

        await _competitionService.UpdateCompetitionAsync(reloadedCompetition);
        List<CompetitionParticipant> rowsAfterUpdate = await LoadCompetitionParticipantRowsAsync(competition.Id);

        Assert.Equal(["rows-a", "rows-b", "rows-c"], rowsAfterAdd.Select(row => row.ParticipantIdentifierName).OrderBy(name => name).ToArray());
        Assert.Equal(["rows-a", "rows-b", "rows-c"], rowsBeforeUpdate.Select(row => row.ParticipantIdentifierName).OrderBy(name => name).ToArray());
        Assert.Equal(["rows-a", "rows-b", "rows-c"], rowsAfterUpdate.Select(row => row.ParticipantIdentifierName).OrderBy(name => name).ToArray());
    }

    [Fact]
    public async Task StartCompetitionAsync_MatchWithTwoParticipants_GeneratesSingleFirstRoundPairing()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["match-p1", "match-p2"]);

        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);
        Assert.Equal(CompetitionState.Ongoing, startedCompetition.State);
        Assert.Equal(1, startedCompetition.CurrentRound);
        Assert.Equal(1, pairing.RoundNumber);
        Assert.Equal(1, pairing.TableNumber);
        Assert.Contains(pairing.WhitePlayerIdentifierName, new[] { "match-p1", "match-p2" });
        Assert.Contains(pairing.BlackPlayerIdentifierName, new[] { "match-p1", "match-p2" });
        Assert.NotEqual(pairing.WhitePlayerIdentifierName, pairing.BlackPlayerIdentifierName);
    }

    [Fact]
    public async Task AdvanceRoundAsync_OngoingCompetition_AddsNextRoundPairingsAndPersistsState()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["a1", "a2", "a3", "a4"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        Competition advancedCompetition = await _competitionService.AdvanceRoundAsync(startedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(CompetitionState.Ongoing, advancedCompetition.State);
        Assert.Equal(2, advancedCompetition.CurrentRound);
        Assert.Equal(2, advancedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 1));
        Assert.Equal(2, advancedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 2));
        Assert.Equal(4, advancedCompetition.CompetitionPairings.Count);

        Assert.Equal(CompetitionState.Ongoing, persistedCompetition.State);
        Assert.Equal(2, persistedCompetition.CurrentRound);
        Assert.Equal(2, persistedCompetition.CompetitionPairings.Count(pairing => pairing.RoundNumber == 2));
    }

    [Fact]
    public async Task AdvanceRoundAsync_WhenCompetitionIsNotOngoing_ReturnsCompetitionUnchanged()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["b1", "b2", "b3", "b4"]);

        Competition resultCompetition = await _competitionService.AdvanceRoundAsync(competition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(CompetitionState.Starting, resultCompetition.State);
        Assert.Equal(0, resultCompetition.CurrentRound);
        Assert.Empty(resultCompetition.CompetitionPairings);

        Assert.Equal(CompetitionState.Starting, persistedCompetition.State);
        Assert.Equal(0, persistedCompetition.CurrentRound);
        Assert.Empty(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_OngoingCompetitionWithResults_CreatesGamesAndPersistsWinner()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["c1", "c2"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);
        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);
        pairing.Result = GameResult.WhiteWins;

        Competition completedCompetition = await _competitionService.CompleteCompetitionAsync(startedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        CompetitionPairing completedPairing = Assert.Single(completedCompetition.CompetitionPairings);
        CompetitionPairing persistedPairing = Assert.Single(persistedCompetition.CompetitionPairings);

        Assert.Equal(CompetitionState.Completed, completedCompetition.State);
        Assert.Equal(completedPairing.WhitePlayerIdentifierName, completedCompetition.WinnerId);
        Assert.NotNull(completedPairing.GameId);
        Assert.Equal(GameResult.WhiteWins, completedPairing.Result);

        Assert.Equal(CompetitionState.Completed, persistedCompetition.State);
        Assert.Equal(completedCompetition.WinnerId, persistedCompetition.WinnerId);
        Assert.NotNull(persistedPairing.GameId);
        Assert.Equal(GameResult.WhiteWins, persistedPairing.Result);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_WhenCompetitionIsNotOngoing_ReturnsCompetitionUnchanged()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["d1", "d2", "d3", "d4"]);

        Competition resultCompetition = await _competitionService.CompleteCompetitionAsync(competition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(CompetitionState.Starting, resultCompetition.State);
        Assert.Null(resultCompetition.WinnerId);
        Assert.Empty(resultCompetition.CompetitionPairings);

        Assert.Equal(CompetitionState.Starting, persistedCompetition.State);
        Assert.Null(persistedCompetition.WinnerId);
        Assert.Empty(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task GetCompetitionByIdAsync_PopulatesPairingGame_WhenGameExists_AndLeavesOnlyGameId_WhenDeleted()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["hydration-white", "hydration-black"]);

        Game game = new()
        {
            WhitePlayerId = "hydration-white",
            BlackPlayerId = "hydration-black",
            TimeFormat = competition.TimeFormat,
            Result = GameResult.WhiteWins,
        };

        await _databaseService.AddAsync(game);
        await _databaseService.AddAsync(new CompetitionPairing
        {
            CompetitionId = competition.Id,
            RoundNumber = 1,
            TableNumber = 1,
            WhitePlayerIdentifierName = "hydration-white",
            BlackPlayerIdentifierName = "hydration-black",
            GameId = game.Id,
        });

        competition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        CompetitionPairing pairingWithGame = Assert.Single(competition.CompetitionPairings);
        Assert.NotNull(pairingWithGame.Competition);
        Assert.Equal(pairingWithGame.CompetitionId, competition.Id);
        int pairingWithGameId = Assert.IsType<int>(pairingWithGame.GameId);
        Assert.Equal(game.Id, pairingWithGameId);

        bool deleted = await _gameService.DeleteGameAsync(pairingWithGameId);

        Competition competitionAfterGameDeletion = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        CompetitionPairing pairingWithoutGame = Assert.Single(competitionAfterGameDeletion.CompetitionPairings);
        Assert.NotNull(pairingWithoutGame.Competition);
        Assert.Equal(pairingWithoutGame.CompetitionId, competition.Id);
        Assert.True(deleted);
        int pairingWithoutGameId = Assert.IsType<int>(pairingWithoutGame.GameId);
        Assert.Equal(game.Id, pairingWithoutGameId);
        Assert.False(_gameService.GameExists(pairingWithoutGameId));
    }

    [Fact]
    public async Task GetCompetitionByIdAsync_WhenCompetitionDoesNotExist_ReturnsNull()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(99999);

        Assert.Null(competition);
    }

    [Fact]
    public async Task CreateCompetitionAsync_PersistsCompetitionRowWithoutParticipants()
    {
        Competition competition = new()
        {
            Title = "Created Competition",
            City = "Recife",
            ChiefArbiter = "Alex Silva",
            Federation = "BRA",
            Type = CompetitionType.ChessMatch,
            State = CompetitionState.Starting,
            TimeFormat = TimeFormat.Blitz,
            NumberOfRounds = 1,
        };

        Competition createdCompetition = await _competitionService.CreateCompetitionAsync(competition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(createdCompetition.Id))!;

        Assert.Equal(createdCompetition.Id, persistedCompetition.Id);
        Assert.Equal("Created Competition", persistedCompetition.Title);
        Assert.Equal("Recife", persistedCompetition.City);
        Assert.Equal("Alex Silva", persistedCompetition.ChiefArbiter);
        Assert.Equal("BRA", persistedCompetition.Federation);
        Assert.Empty(persistedCompetition.CompetitionParticipants);
        Assert.Empty(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task UpdateCompetitionAsync_PersistsCompetitionMetadataWinnerIdAndCompletedAt()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["meta-a", "meta-b"]);
        DateTime completedAt = _timeProvider.GetUtcNow().UtcDateTime;

        competition.WinnerId = "meta-b";
        competition.CompletedAt = completedAt;
        competition.City = "São Paulo";
        competition.ChiefArbiter = "Morgan Lee";
        competition.Federation = "BRA";

        await _competitionService.UpdateCompetitionAsync(competition);

        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal("meta-b", persistedCompetition.WinnerId);
        Assert.Equal(completedAt, persistedCompetition.CompletedAt);
        Assert.Equal("São Paulo", persistedCompetition.City);
        Assert.Equal("Morgan Lee", persistedCompetition.ChiefArbiter);
        Assert.Equal("BRA", persistedCompetition.Federation);
        Assert.Equal(2, persistedCompetition.CompetitionParticipants.Count);
    }

    [Fact]
    public async Task UpdateCompetitionAsync_PersistsNullOptionalFields()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["null-a", "null-b"]);

        competition.Description = null;
        competition.City = null;
        competition.ChiefArbiter = null;
        competition.Federation = null;
        competition.ByeValue = null;
        competition.CompletedAt = null;
        competition.WinnerId = null;

        await _competitionService.UpdateCompetitionAsync(competition);

        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Null(persistedCompetition.Description);
        Assert.Null(persistedCompetition.City);
        Assert.Null(persistedCompetition.ChiefArbiter);
        Assert.Null(persistedCompetition.Federation);
        Assert.Null(persistedCompetition.ByeValue);
        Assert.Null(persistedCompetition.CompletedAt);
        Assert.Null(persistedCompetition.WinnerId);
    }

    [Fact]
    public async Task DeleteCompetitionAsync_WhenCompetitionDoesNotExist_ReturnsFalse()
    {
        bool deleted = await _competitionService.DeleteCompetitionAsync(99999);
        List<Competition> competitions = await _competitionService.GetAllCompetitionsAsync();

        Assert.False(deleted);
        Assert.Single(competitions);
    }

    [Fact]
    public async Task AddParticipantAsync_WhenPlayerDoesNotExist_PersistsIdentifierWithDefaultRating()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["ghost-a", "ghost-b"]);

        Competition updatedCompetition = await _competitionService.AddParticipantAsync(competition, "ghost-c");
        CompetitionParticipant participant = Assert.Single(updatedCompetition.CompetitionParticipants.Where(p => p.ParticipantIdentifierName == "ghost-c"));

        Assert.Equal("ghost-c", participant.ParticipantIdentifierName);
        Assert.Equal(_clubService.CurrentClub.Rating, participant.OldRating.Rating);
        Assert.Equal(_clubService.CurrentClub.Deviation, participant.OldRating.Deviation);
        Assert.Equal(_clubService.CurrentClub.Volatility, participant.OldRating.Volatility);
    }

    [Fact]
    public async Task AddParticipantAsync_WhenIdentifierDiffersOnlyByCase_DoesNotCreateDuplicate()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        Competition updatedCompetition = await _competitionService.AddParticipantAsync(competition!, "WHITE-PLAYER");
        List<CompetitionParticipant> rows = await LoadCompetitionParticipantRowsAsync(1);

        Assert.Equal(2, updatedCompetition.CompetitionParticipants.Count);
        Assert.Equal(2, rows.Count);
        Assert.Single(rows.Where(row => row.ParticipantIdentifierName == "white-player"));
    }

    [Fact]
    public async Task RemoveParticipantAsync_WhenCompetitionHasNoMatchingParticipant_DoesNotDeleteAnything()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        await _competitionService.RemoveParticipantAsync(competition!, "not-present");

        List<CompetitionParticipant> rows = await LoadCompetitionParticipantRowsAsync(1);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public async Task UndoRoundAsync_OngoingCompetitionWithRoundOne_RevertsToStartingAndDeletesRoundPairings()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["u1", "u2", "u3", "u4"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        Competition undoneCompetition = await _competitionService.UndoRoundAsync(startedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(CompetitionState.Starting, undoneCompetition.State);
        Assert.Equal(0, undoneCompetition.CurrentRound);
        Assert.Empty(undoneCompetition.CompetitionPairings);
        Assert.Equal(CompetitionState.Starting, persistedCompetition.State);
        Assert.Equal(0, persistedCompetition.CurrentRound);
        Assert.Empty(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task UndoRoundAsync_OngoingCompetitionWithMultipleRounds_DeletesCurrentAndLaterRoundsOnly()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["uu1", "uu2", "uu3", "uu4"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);
        Competition advancedCompetition = await _competitionService.AdvanceRoundAsync(startedCompetition);

        Competition undoneCompetition = await _competitionService.UndoRoundAsync(advancedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(1, undoneCompetition.CurrentRound);
        Assert.Equal(CompetitionState.Ongoing, undoneCompetition.State);
        Assert.Equal(2, undoneCompetition.CompetitionPairings.Count);
        Assert.All(undoneCompetition.CompetitionPairings, pairing => Assert.Equal(1, pairing.RoundNumber));
        Assert.Equal(1, persistedCompetition.CurrentRound);
        Assert.Equal(2, persistedCompetition.CompetitionPairings.Count);
        Assert.All(persistedCompetition.CompetitionPairings, pairing => Assert.Equal(1, pairing.RoundNumber));
    }

    [Fact]
    public async Task UndoRoundAsync_WhenCompetitionIsNotOngoing_ReturnsCompetitionUnchanged()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 3, participantIdentifiers: ["ux1", "ux2", "ux3", "ux4"]);

        Competition resultCompetition = await _competitionService.UndoRoundAsync(competition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(CompetitionState.Starting, resultCompetition.State);
        Assert.Equal(0, resultCompetition.CurrentRound);
        Assert.Empty(resultCompetition.CompetitionPairings);
        Assert.Equal(CompetitionState.Starting, persistedCompetition.State);
        Assert.Equal(0, persistedCompetition.CurrentRound);
        Assert.Empty(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task AdvanceRoundAsync_WhenAtLastRound_DoesNotCreateExtraRoundsBeyondNumberOfRounds()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["last-a", "last-b"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);
        Competition advancedCompetition = await _competitionService.AdvanceRoundAsync(startedCompetition);
        Competition persistedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;

        Assert.Equal(1, advancedCompetition.CurrentRound);
        Assert.Single(advancedCompetition.CompetitionPairings);
        Assert.Single(persistedCompetition.CompetitionPairings);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_RatedCompetition_PersistsParticipantNewRatings()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["rated-a", "rated-b"]);
        competition.IsRated = true;
        await _competitionService.UpdateCompetitionAsync(competition);

        Competition startedCompetition = await _competitionService.StartCompetitionAsync((await _competitionService.GetCompetitionByIdAsync(competition.Id))!);
        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);
        pairing.Result = GameResult.WhiteWins;

        Competition completedCompetition = await _competitionService.CompleteCompetitionAsync(startedCompetition);
        CompetitionParticipant whiteParticipant = Assert.Single(completedCompetition.CompetitionParticipants.Where(p => p.ParticipantIdentifierName == pairing.WhitePlayerIdentifierName));
        CompetitionParticipant blackParticipant = Assert.Single(completedCompetition.CompetitionParticipants.Where(p => p.ParticipantIdentifierName == pairing.BlackPlayerIdentifierName));

        Assert.NotNull(whiteParticipant.NewRating);
        Assert.NotNull(blackParticipant.NewRating);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_WhenPairingAlreadyHasGameId_DoesNotCreateDuplicateGame()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["existing-a", "existing-b"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);
        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);

        Game existingGame = new()
        {
            WhitePlayerId = pairing.WhitePlayerIdentifierName,
            BlackPlayerId = pairing.BlackPlayerIdentifierName,
            TimeFormat = startedCompetition.TimeFormat,
            Result = GameResult.Draw,
        };

        await _databaseService.AddAsync(existingGame);
        pairing.GameId = existingGame.Id;
        pairing.Result = GameResult.Draw;
        await _databaseService.UpdateAsync(pairing);

        Competition completedCompetition = await _competitionService.CompleteCompetitionAsync(startedCompetition);
        List<Game> games = await _databaseService.GetAllAsync<Game>();
        CompetitionPairing completedPairing = Assert.Single(completedCompetition.CompetitionPairings);

        Assert.Single(games);
        Assert.Equal(existingGame.Id, completedPairing.GameId);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_WhenParticipantPlayerWasDeleted_SkipsNewRatingButStillCompletesCompetition()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["gone-a", "gone-b"]);
        competition.IsRated = true;
        await _competitionService.UpdateCompetitionAsync(competition);

        Competition startedCompetition = await _competitionService.StartCompetitionAsync((await _competitionService.GetCompetitionByIdAsync(competition.Id))!);
        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);
        pairing.Result = GameResult.WhiteWins;

        await _playerService.DeletePlayerAsync(pairing.WhitePlayerIdentifierName);

        Competition completedCompetition = await _competitionService.CompleteCompetitionAsync(startedCompetition);
        CompetitionParticipant missingPlayerParticipant = Assert.Single(completedCompetition.CompetitionParticipants.Where(p => p.ParticipantIdentifierName == pairing.WhitePlayerIdentifierName));

        Assert.Equal(CompetitionState.Completed, completedCompetition.State);
        Assert.Null(missingPlayerParticipant.NewRating);
        Assert.Equal(pairing.WhitePlayerIdentifierName, completedCompetition.WinnerId);
    }

    [Fact]
    public async Task CompleteCompetitionAsync_WithByePairing_DoesNotCreateGameForBye()
    {
        Competition competition = await CreateStartingSwissCompetitionAsync(numberOfRounds: 1, participantIdentifiers: ["bye-a", "bye-b", "bye-c"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);

        Competition completedCompetition = await _competitionService.CompleteCompetitionAsync(startedCompetition);
        List<Game> games = await _databaseService.GetAllAsync<Game>();

        Assert.Equal(CompetitionState.Completed, completedCompetition.State);
        Assert.DoesNotContain(completedCompetition.CompetitionPairings, pairing => pairing.IsByePairing() && pairing.GameId != null);
        Assert.True(games.Count <= completedCompetition.CompetitionPairings.Count(pairing => !pairing.IsByePairing()));
    }

    [Fact]
    public async Task GetCompetitionByIdAsync_AfterParticipantDeletion_ReturnsAggregateWithoutDeletedParticipantRow()
    {
        Competition? competition = await _competitionService.GetCompetitionByIdAsync(1);

        await _competitionService.RemoveParticipantAsync(competition!, "white-player");

        Competition reloadedCompetition = (await _competitionService.GetCompetitionByIdAsync(1))!;

        Assert.DoesNotContain(reloadedCompetition.CompetitionParticipants, participant => participant.ParticipantIdentifierName == "white-player");
        Assert.Single(reloadedCompetition.ComputedStandings);
    }

    [Fact]
    public async Task GetCompetitionByIdAsync_AfterPairingUpdates_ReturnsPersistedGameIdAndResult()
    {
        Competition competition = await CreateStartingCompetitionAsync(
            CompetitionType.ChessMatch,
            numberOfRounds: 1,
            participantIdentifiers: ["persist-a", "persist-b"]);
        Competition startedCompetition = await _competitionService.StartCompetitionAsync(competition);
        CompetitionPairing pairing = Assert.Single(startedCompetition.CompetitionPairings);

        pairing.GameId = 42;
        pairing.Result = GameResult.BlackWins;
        await _databaseService.UpdateAsync(pairing);

        Competition reloadedCompetition = (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;
        CompetitionPairing reloadedPairing = Assert.Single(reloadedCompetition.CompetitionPairings);

        Assert.Equal(42, reloadedPairing.GameId);
        Assert.Equal(GameResult.BlackWins, reloadedPairing.Result);
    }

    private async Task SetupScenarioInDb()
    {
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

        Player newPlayer = new()
        {
            IdentifierName = "new-player",
            DisplayName = "New Player",
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
            CompetitionId = competition.Id,
            ParticipantIdentifierName = whitePlayer.IdentifierName,
            OldRating = new ChessRating(),
        };

        CompetitionParticipant blackParticipant = new()
        {
            Competition = competition,
            CompetitionId = competition.Id,
            ParticipantIdentifierName = blackPlayer.IdentifierName,
            OldRating = new ChessRating(),
        };

        await _databaseService.AddAsync(whitePlayer);
        await _databaseService.AddAsync(blackPlayer);
        await _databaseService.AddAsync(newPlayer);
        await _databaseService.AddAsync(competition);
        await _databaseService.AddAsync(whiteParticipant);
        await _databaseService.AddAsync(blackParticipant);
    }

    private async Task<Competition> CreateStartingSwissCompetitionAsync(int numberOfRounds, params string[] participantIdentifiers)
    {
        return await CreateStartingCompetitionAsync(CompetitionType.SwissSystemTournament, numberOfRounds, participantIdentifiers);
    }

    private async Task<List<CompetitionParticipant>> LoadCompetitionParticipantRowsAsync(int competitionId)
    {
        return await _databaseService.GetWhereAsync<CompetitionParticipant>(participant => participant.CompetitionId == competitionId);
    }

    private async Task<Competition> CreateStartingCompetitionAsync(CompetitionType type, int numberOfRounds, params string[] participantIdentifiers)
    {
        Competition competition = new()
        {
            Title = $"Competition {Guid.NewGuid():N}",
            Type = type,
            State = CompetitionState.Starting,
            TimeFormat = TimeFormat.Blitz,
            NumberOfRounds = numberOfRounds,
            ByeValue = 1,
        };

        await _databaseService.AddAsync(competition);

        foreach (string identifier in participantIdentifiers)
        {
            Player player = new()
            {
                IdentifierName = identifier,
                DisplayName = identifier,
                BlitzRating = new ChessRating { Rating = 1500 },
            };

            await _databaseService.AddAsync(player);
            await _databaseService.AddAsync(new CompetitionParticipant
            {
                Competition = competition,
                CompetitionId = competition.Id,
                ParticipantIdentifierName = identifier,
                OldRating = player.BlitzRating,
            });
        }

        return (await _competitionService.GetCompetitionByIdAsync(competition.Id))!;
    }

    private static void SetCurrentClub(ClubService clubService, ClubPreferences clubPreferences)
    {
        PropertyInfo? currentClubProperty = typeof(ClubService).GetProperty(
            nameof(ClubService.CurrentClub),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        currentClubProperty!.SetValue(clubService, clubPreferences);
    }
}
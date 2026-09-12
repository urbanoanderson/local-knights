using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;
using Moq;

namespace LocalKnights.WindowsDesktop.Tests;

public sealed class CompetitionViewModelTests
{
    [Fact]
    public void CompetitionFederation_LowercaseValue_UppercasesAndPersistsValue()
    {
        Competition competition = new()
        {
            Id = 42,
            Title = "Federation test",
            Type = CompetitionType.SwissSystemTournament,
            State = CompetitionState.Starting,
            TimeFormat = TimeFormat.Blitz,
        };
        Mock<ICompetitionService> competitionServiceMock = CreateCompetitionServiceMock(competition);
        CompetitionViewModel viewModel = new(
            logger: NullLogger.Instance,
            pageNavigationService: CreatePageNavigationServiceMock().Object,
            clubService: CreateClubServiceMock().Object,
            competitionService: competitionServiceMock.Object,
            playerService: CreatePlayerServiceMock().Object,
            fideApiService: CreateFideApiServiceMock().Object,
            gameService: CreateGameServiceMock().Object,
            fileSystemService: CreateFileSystemServiceMock().Object,
            messageBoxService: CreateMessageBoxServiceMock(CustomMessageBoxResult.OK).Object,
            timeProvider: TimeProvider.System);
        SetCompetition(viewModel, competition);

        viewModel.CompetitionFederation = "bra";

        Assert.Equal("BRA", viewModel.CompetitionFederation);
        competitionServiceMock.Verify(
            service => service.UpdateCompetitionAsync(It.Is<Competition>(updated => updated.Federation == "BRA")),
            Times.Once);
    }

    [Fact]
    public void StartCompetitionCommand_SwissCompetitionWithTooManyRounds_ShowsWarningBeforeStarting()
    {
        Competition competition = new()
        {
            Id = 42,
            Title = "Swiss test",
            Type = CompetitionType.SwissSystemTournament,
            State = CompetitionState.Starting,
            TimeFormat = TimeFormat.Blitz,
            NumberOfRounds = 5,
            CompetitionParticipants = new Collection<CompetitionParticipant>
            {
                new() { CompetitionId = 42, ParticipantIdentifierName = "p1", OldRating = new ChessRating() },
                new() { CompetitionId = 42, ParticipantIdentifierName = "p2", OldRating = new ChessRating() },
            }
        };

        Mock<IMessageBoxService> messageBoxServiceMock = CreateMessageBoxServiceMock(CustomMessageBoxResult.OK);
        Mock<ICompetitionService> competitionServiceMock = CreateCompetitionServiceMock(competition);

        CompetitionViewModel viewModel = new CompetitionViewModel(
            logger: NullLogger.Instance,
            pageNavigationService: CreatePageNavigationServiceMock().Object,
            clubService: CreateClubServiceMock().Object,
            competitionService: competitionServiceMock.Object,
            playerService: CreatePlayerServiceMock().Object,
            fideApiService: CreateFideApiServiceMock().Object,
            gameService: CreateGameServiceMock().Object,
            fileSystemService: CreateFileSystemServiceMock().Object,
            messageBoxService: messageBoxServiceMock.Object,
            timeProvider: TimeProvider.System);

        SetCompetition(viewModel, competition);

        viewModel.ParticipantTableRows.Add(new CompetitionPageParticipantTableItem
        {
            IdentifierName = "p1",
            DisplayName = "Player 1",
            RatingDisplay = "1500",
            CanNavigateToPlayer = true,
        });
        viewModel.ParticipantTableRows.Add(new CompetitionPageParticipantTableItem
        {
            IdentifierName = "p2",
            DisplayName = "Player 2",
            RatingDisplay = "1500",
            CanNavigateToPlayer = true,
        });

        Assert.True(viewModel.StartCompetitionCommand.CanExecute(null));

        viewModel.StartCompetitionCommand.Execute(null);

        messageBoxServiceMock.Verify(
            service => service.Show(
                It.IsAny<string>(),
                It.IsAny<string>(),
                CustomMessageBoxButton.OKCancel,
                CustomMessageBoxImage.Warning),
            Times.Once);

        competitionServiceMock.Verify(service => service.StartCompetitionAsync(competition), Times.Once);
    }

    [Fact]
    public void ExportToTrf16Command_IncompleteCompetition_ShowsWarningWithoutWritingFile()
    {
        Competition competition = new()
        {
            Id = 42,
            Title = "Ongoing competition",
            State = CompetitionState.Ongoing,
            TimeFormat = TimeFormat.Blitz,
        };
        Mock<IMessageBoxService> messageBoxServiceMock = CreateMessageBoxServiceMock(CustomMessageBoxResult.OK);
        Mock<IFileSystemService> fileSystemServiceMock = CreateFileSystemServiceMock();
        CompetitionViewModel viewModel = new CompetitionViewModel(
            logger: NullLogger.Instance,
            pageNavigationService: CreatePageNavigationServiceMock().Object,
            clubService: CreateClubServiceMock().Object,
            competitionService: CreateCompetitionServiceMock(competition).Object,
            playerService: CreatePlayerServiceMock().Object,
            fideApiService: CreateFideApiServiceMock().Object,
            gameService: CreateGameServiceMock().Object,
            fileSystemService: fileSystemServiceMock.Object,
            messageBoxService: messageBoxServiceMock.Object,
            timeProvider: TimeProvider.System);
        SetCompetition(viewModel, competition);

        viewModel.ExportToTrf16Command.Execute(null);

        messageBoxServiceMock.Verify(
            service => service.ShowWarning(
                LocalizationHelper.GetString("TrfExportCompletedCompetitionOnlyMessage"),
                LocalizationHelper.GetString("ExportButtonLabel")),
            Times.Once);
        fileSystemServiceMock.VerifyNoOtherCalls();
    }

    private static void SetCompetition(CompetitionViewModel viewModel, Competition competition)
    {
        PropertyInfo? competitionProperty = typeof(CompetitionViewModel).GetProperty(
            nameof(CompetitionViewModel.Competition),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        competitionProperty!.SetValue(viewModel, competition);
    }

    private static Mock<ICompetitionService> CreateCompetitionServiceMock(Competition startCompetitionResult)
    {
        Mock<ICompetitionService> mock = new();
        mock.Setup(service => service.GetCompetitionByIdAsync(It.IsAny<int>())).ReturnsAsync(startCompetitionResult);
        mock.Setup(service => service.GetAllCompetitionsAsync()).ReturnsAsync([]);
        mock.Setup(service => service.GetCompetitionsCountAsync(It.IsAny<string?>())).ReturnsAsync(0);
        mock.Setup(service => service.GetCompetitionsPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<SortParameter?>())).ReturnsAsync([]);
        mock.Setup(service => service.CreateCompetitionAsync(It.IsAny<Competition>())).ReturnsAsync((Competition competition) => competition);
        mock.Setup(service => service.UpdateCompetitionAsync(It.IsAny<Competition>())).ReturnsAsync((Competition competition) => competition);
        mock.Setup(service => service.DeleteCompetitionAsync(It.IsAny<int>())).ReturnsAsync(true);
        mock.Setup(service => service.AddParticipantAsync(It.IsAny<Competition>(), It.IsAny<string>())).ReturnsAsync((Competition competition, string _) => competition);
        mock.Setup(service => service.RemoveParticipantAsync(It.IsAny<Competition>(), It.IsAny<string>())).ReturnsAsync((Competition competition, string _) => competition);
        mock.Setup(service => service.StartCompetitionAsync(It.IsAny<Competition>())).ReturnsAsync(startCompetitionResult);
        mock.Setup(service => service.UndoRoundAsync(It.IsAny<Competition>())).ReturnsAsync((Competition competition) => competition);
        mock.Setup(service => service.AdvanceRoundAsync(It.IsAny<Competition>())).ReturnsAsync((Competition competition) => competition);
        mock.Setup(service => service.CompleteCompetitionAsync(It.IsAny<Competition>())).ReturnsAsync((Competition competition) => competition);
        return mock;
    }

    private static Mock<IMessageBoxService> CreateMessageBoxServiceMock(CustomMessageBoxResult nextResult)
    {
        Mock<IMessageBoxService> mock = new();
        mock.Setup(service => service.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomMessageBoxButton>(), It.IsAny<CustomMessageBoxImage>()))
            .Returns(nextResult);
        mock.Setup(service => service.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(nextResult);
        mock.Setup(service => service.ShowError(It.IsAny<string>(), It.IsAny<string>())).Returns(nextResult);
        mock.Setup(service => service.ShowInfo(It.IsAny<string>(), It.IsAny<string>())).Returns(nextResult);
        mock.Setup(service => service.ShowWarning(It.IsAny<string>(), It.IsAny<string>())).Returns(nextResult);
        return mock;
    }

    private static Mock<IFileSystemService> CreateFileSystemServiceMock()
    {
        return new Mock<IFileSystemService>();
    }

    private static Mock<IFideApiService> CreateFideApiServiceMock()
    {
        Mock<IFideApiService> mock = new();
        mock.Setup(service => service.GetPlayerRatingsAsync(It.IsAny<string>()))
            .ReturnsAsync(new Dictionary<TimeFormat, double>());
        return mock;
    }

    private static Mock<IPageNavigationService> CreatePageNavigationServiceMock()
    {
        Mock<IPageNavigationService> mock = new();
        mock.SetupGet(service => service.ActivePage).Returns((BaseAppViewModel)null!);
        return mock;
    }

    private static Mock<IClubService> CreateClubServiceMock()
    {
        ClubPreferences currentClub = new()
        {
            IdentifierName = "club",
            DisplayName = "Club",
            UpdatedAt = DateTime.UtcNow,
        };

        Mock<IClubService> mock = new();
        mock.SetupGet(service => service.CurrentClub).Returns(currentClub);
        mock.Setup(service => service.ClubExists(It.IsAny<string>())).Returns(false);
        mock.Setup(service => service.ClubSignInAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ClubPreferences?)null);
        mock.Setup(service => service.CreateClubAsync(It.IsAny<ClubPreferences>(), It.IsAny<string>())).ReturnsAsync((ClubPreferences club, string _) => club);
        mock.Setup(service => service.DeleteClubAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(service => service.GetExistingClubIdentifiers()).Returns([]);
        mock.Setup(service => service.ClubSignOutAsync()).Returns(Task.CompletedTask);
        mock.Setup(service => service.UpdateClubAsync(It.IsAny<ClubPreferences>())).ReturnsAsync((ClubPreferences club) => club);
        mock.Setup(service => service.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<IPlayerService> CreatePlayerServiceMock()
    {
        Mock<IPlayerService> mock = new();
        mock.Setup(service => service.CreatePlayerAsync(It.IsAny<Player>())).ReturnsAsync((Player player) => player);
        mock.Setup(service => service.DeletePlayerAsync(It.IsAny<string>())).ReturnsAsync(true);
        mock.Setup(service => service.GetAllPlayersAsync()).ReturnsAsync([]);
        mock.Setup(service => service.GetPlayerTimeFormatPosition(It.IsAny<string>(), It.IsAny<TimeFormat>())).Returns(0);
        mock.Setup(service => service.GetPlayerByIdentifierAsync(It.IsAny<string>())).ReturnsAsync((Player?)null);
        mock.Setup(service => service.GetPlayerRatedGamesCountSinceLastYear(It.IsAny<string>(), It.IsAny<TimeFormat?>())).ReturnsAsync(0);
        mock.Setup(service => service.GetPlayersCountAsync(It.IsAny<string?>())).ReturnsAsync(0);
        mock.Setup(service => service.GetPlayersPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<SortParameter?>())).ReturnsAsync([]);
        mock.Setup(service => service.PlayerIdentifierExistsAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(false);
        mock.Setup(service => service.SearchPlayersAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync([]);
        mock.Setup(service => service.UpdatePlayerAsync(It.IsAny<Player>())).ReturnsAsync((Player player) => player);
        return mock;
    }

    private static Mock<IGameService> CreateGameServiceMock()
    {
        Mock<IGameService> mock = new();
        mock.Setup(service => service.GameExists(It.IsAny<int>())).Returns(false);
        mock.Setup(service => service.GetAllGamesAsync()).ReturnsAsync([]);
        mock.Setup(service => service.GetGameByIdAsync(It.IsAny<int>())).ReturnsAsync((Game?)null);
        mock.Setup(service => service.GetAllPlayersAsync()).ReturnsAsync([]);
        mock.Setup(service => service.CreateGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeFormat>(), It.IsAny<GameResult>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .ReturnsAsync(new Game());
        mock.Setup(service => service.UpdateGamePgnAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(true);
        mock.Setup(service => service.DeleteGameAsync(It.IsAny<int>())).ReturnsAsync(true);
        mock.Setup(service => service.DeleteGamesAsync(It.IsAny<IEnumerable<int>>())).Returns(Task.CompletedTask);
        mock.Setup(service => service.GetGamesPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SortParameter?>())).ReturnsAsync([]);
        mock.Setup(service => service.GetGamesCountAsync()).ReturnsAsync(0);
        mock.Setup(service => service.GetCompetitionIdForGameAsync(It.IsAny<int>())).ReturnsAsync((int?)null);
        return mock;
    }
}
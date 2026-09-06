using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Unit tests for <see cref="ClubPreferencesViewModel"/>.
/// </summary>
public sealed class ClubPreferencesViewModelTests
{
    [Fact]
    public void Constructor_PopulatesValuesFromCurrentClub()
    {
        DateTime createdUtc = new DateTime(2026, 08, 10, 10, 00, 00, DateTimeKind.Utc);
        DateTime updatedUtc = new DateTime(2026, 08, 16, 14, 30, 00, DateTimeKind.Utc);
        ClubPreferences clubPreferences = CreateCurrentClub(createdUtc, updatedUtc);

        ClubPreferencesViewModel viewModel = CreateViewModel(clubPreferences);

        Assert.Equal(clubPreferences.DisplayName, viewModel.DisplayName);
        Assert.Equal(clubPreferences.Description, viewModel.Description);
        Assert.Equal(clubPreferences.City, viewModel.City);
        Assert.Equal(clubPreferences.Federation, viewModel.Federation);
        Assert.Equal(clubPreferences.DefaultChiefArbiter, viewModel.DefaultChiefArbiter);
        Assert.Equal(clubPreferences.MinRating, viewModel.MinRating);
        Assert.Equal(clubPreferences.MaxRating, viewModel.MaxRating);
        Assert.Equal(createdUtc.ToLocalTime().ToString("G"), viewModel.CreatedAtText);
        Assert.Equal(updatedUtc.ToLocalTime().ToString("G"), viewModel.UpdatedAtText);
    }

    [Fact]
    public void MinAndMaxRating_Setters_ClampBeforeValidation()
    {
        ClubPreferences currentClub = CreateCurrentClub();
        ClubPreferencesViewModel viewModel = CreateViewModel(currentClub);

        viewModel.MinRating = CommonConstants.DefaultMinRatingLowerBound - 50;
        viewModel.MaxRating = CommonConstants.DefaultMaxRatingUpperBound + 50;

        Assert.Equal(CommonConstants.DefaultMinRatingLowerBound, viewModel.MinRating);
        Assert.Equal(CommonConstants.DefaultMaxRatingUpperBound, viewModel.MaxRating);
    }

    [Fact]
    public async Task SaveCommand_WithValidInput_PreservesCreatedAtAndUpdatesUpdatedAt()
    {
        DateTime createdUtc = new DateTime(2026, 08, 10, 10, 00, 00, DateTimeKind.Utc);
        DateTime updatedUtc = new DateTime(2026, 08, 12, 09, 00, 00, DateTimeKind.Utc);
        ClubPreferences currentClub = CreateCurrentClub(createdUtc, updatedUtc);
        Mock<IClubService> clubServiceMock = CreateClubServiceMock(currentClub);
        Mock<IPageNavigationService> pageNavigationServiceMock = CreatePageNavigationServiceMock();
        Mock<IMessageBoxService> messageBoxServiceMock = CreateMessageBoxServiceMock();

        ClubPreferencesViewModel viewModel = CreateViewModel(
            currentClub,
            clubServiceMock,
            pageNavigationServiceMock,
            messageBoxServiceMock);

        viewModel.DisplayName = "Updated Club";
        viewModel.Description = "Updated description";
        viewModel.City = "Recife";
        viewModel.Federation = "bra";
        viewModel.DefaultChiefArbiter = "Alex Silva";
        viewModel.MinRating = 130;
        viewModel.MaxRating = 3850;

        viewModel.SaveCommand.Execute(null);
        await Task.Yield();

        clubServiceMock.Verify(service => service.UpdateClubAsync(
            It.Is<ClubPreferences>(club =>
                club.DisplayName == "Updated Club" &&
                club.Description == "Updated description" &&
                club.City == "Recife" &&
                club.Federation == "BRA" &&
                club.DefaultChiefArbiter == "Alex Silva" &&
                club.CreatedAt == createdUtc &&
                club.UpdatedAt >= updatedUtc &&
                club.MinRating == 130 &&
                club.MaxRating == 3850),
            null), Times.Once);

        pageNavigationServiceMock.Verify(service => service.HardNavigateTo<DashboardViewModel>(), Times.Once);
        messageBoxServiceMock.Verify(service => service.ShowInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static ClubPreferencesViewModel CreateViewModel(
        ClubPreferences currentClub,
        Mock<IClubService>? clubServiceMock = null,
        Mock<IPageNavigationService>? pageNavigationServiceMock = null,
        Mock<IMessageBoxService>? messageBoxServiceMock = null)
    {
        return new ClubPreferencesViewModel(
            logger: NullLogger.Instance,
            pageNavigationService: (pageNavigationServiceMock ?? CreatePageNavigationServiceMock()).Object,
            clubService: (clubServiceMock ?? CreateClubServiceMock(currentClub)).Object,
            messageBoxService: (messageBoxServiceMock ?? CreateMessageBoxServiceMock()).Object);
    }

    private static ClubPreferences CreateCurrentClub(DateTime? createdAt = null, DateTime? updatedAt = null)
    {
        return new ClubPreferences
        {
            IdentifierName = "club",
            DisplayName = "Club",
            Description = "Description",
            City = "Recife",
            Federation = "BRA",
            DefaultChiefArbiter = "Alex Silva",
            CreatedAt = createdAt ?? new DateTime(2026, 08, 10, 10, 00, 00, DateTimeKind.Utc),
            UpdatedAt = updatedAt ?? new DateTime(2026, 08, 12, 09, 00, 00, DateTimeKind.Utc),
            Rating = CommonConstants.DefaultRating,
            MinRating = CommonConstants.DefaultMinRating,
            MaxRating = CommonConstants.DefaultMaxRating,
            Deviation = CommonConstants.DefaultDeviation,
            Volatility = CommonConstants.DefaultVolatility,
            RatingSystem = RatingSystem.Glicko2,
            ProvisionalGames = CommonConstants.DefaultProvisionalGames,
            SocialMediaLinks = ["https://example.com"]
        };
    }

    private static Mock<IClubService> CreateClubServiceMock(ClubPreferences currentClub)
    {
        Mock<IClubService> mock = new();
        mock.SetupGet(service => service.CurrentClub).Returns(currentClub);
        mock.Setup(service => service.UpdateClubAsync(It.IsAny<ClubPreferences>(), It.IsAny<string?>()))
            .ReturnsAsync((ClubPreferences club, string? _) => club);
        mock.Setup(service => service.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(service => service.DeleteClubAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<IPageNavigationService> CreatePageNavigationServiceMock()
    {
        Mock<IPageNavigationService> mock = new();
        mock.SetupGet(service => service.ActivePage).Returns((BaseAppViewModel)null!);
        return mock;
    }

    private static Mock<IMessageBoxService> CreateMessageBoxServiceMock()
    {
        Mock<IMessageBoxService> mock = new();
        mock.Setup(service => service.ShowWarning(It.IsAny<string>(), It.IsAny<string>())).Returns(CustomMessageBoxResult.OK);
        mock.Setup(service => service.ShowError(It.IsAny<string>(), It.IsAny<string>())).Returns(CustomMessageBoxResult.OK);
        mock.Setup(service => service.ShowInfo(It.IsAny<string>(), It.IsAny<string>())).Returns(CustomMessageBoxResult.OK);
        mock.Setup(service => service.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(CustomMessageBoxResult.Yes);
        return mock;
    }
}

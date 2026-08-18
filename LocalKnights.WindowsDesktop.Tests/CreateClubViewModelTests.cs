using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Reflection;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Unit tests for <see cref="CreateClubViewModel"/>.
/// </summary>
public sealed class CreateClubViewModelTests
{
    [Fact]
    public void MinRating_Setter_ClampsToConfiguredBounds()
    {
        CreateClubViewModel viewModel = CreateViewModel();

        viewModel.MinRating = CommonConstants.DefaultMinRatingLowerBound - 50;
        Assert.Equal(CommonConstants.DefaultMinRatingLowerBound, viewModel.MinRating);

        viewModel.MinRating = CommonConstants.DefaultMinRatingUpperBound + 50;
        Assert.Equal(CommonConstants.DefaultMinRatingUpperBound, viewModel.MinRating);
    }

    [Fact]
    public void MaxRating_Setter_ClampsToConfiguredBounds()
    {
        CreateClubViewModel viewModel = CreateViewModel();

        viewModel.MaxRating = CommonConstants.DefaultMaxRatingLowerBound - 50;
        Assert.Equal(CommonConstants.DefaultMaxRatingLowerBound, viewModel.MaxRating);

        viewModel.MaxRating = CommonConstants.DefaultMaxRatingUpperBound + 50;
        Assert.Equal(CommonConstants.DefaultMaxRatingUpperBound, viewModel.MaxRating);
    }

    [Fact]
    public void MinAndMaxRating_Setters_ClampBeforeValidation()
    {
        CreateClubViewModel viewModel = CreateViewModel();

        viewModel.MinRating = CommonConstants.DefaultMinRatingLowerBound - 50;
        viewModel.MaxRating = CommonConstants.DefaultMaxRatingUpperBound + 50;

        Assert.Equal(CommonConstants.DefaultMinRatingLowerBound, viewModel.MinRating);
        Assert.Equal(CommonConstants.DefaultMaxRatingUpperBound, viewModel.MaxRating);
    }

    private static CreateClubViewModel CreateViewModel(
        Mock<IClubService>? clubServiceMock = null,
        Mock<IPageNavigationService>? navigationServiceMock = null,
        Mock<IMessageBoxService>? messageBoxServiceMock = null,
        TimeProvider? timeProvider = null)
    {
        return new CreateClubViewModel(
            logger: NullLogger.Instance,
            pageNavigationService: (navigationServiceMock ?? CreatePageNavigationServiceMock()).Object,
            clubService: (clubServiceMock ?? CreateClubServiceMock()).Object,
            messageBoxService: (messageBoxServiceMock ?? CreateMessageBoxServiceMock()).Object,
            timeProvider: timeProvider ?? TimeProvider.System);
    }

    private static void PopulateValidRequiredFields(CreateClubViewModel viewModel)
    {
        viewModel.IdentifierName = "testclub";
        viewModel.DisplayName = "Test Club";
        viewModel.Password = "secret";
        viewModel.ConfirmPassword = "secret";
        viewModel.Rating = 1500;
        viewModel.MinRating = CommonConstants.DefaultMinRating;
        viewModel.MaxRating = CommonConstants.DefaultMaxRating;
        viewModel.Deviation = CommonConstants.DefaultDeviation;
        viewModel.Volatility = CommonConstants.DefaultVolatility;
    }

    private static Mock<IClubService> CreateClubServiceMock()
    {
        Mock<IClubService> mock = new();
        mock.SetupGet(service => service.CurrentClub).Returns((ClubPreferences?)null!);
        mock.Setup(service => service.ClubExists(It.IsAny<string>())).Returns(false);
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
        return mock;
    }
}

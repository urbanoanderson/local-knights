using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LocalKnights.WindowsDesktop.Tests;

public sealed class StartViewModelTests
{
    [Fact]
    public async Task SignInCommand_WhileSignInIsRunning_ShowsSigningInText()
    {
        using ManualResetEventSlim signInStarted = new();
        using ManualResetEventSlim allowSignInToFinish = new();
        Mock<IClubService> clubServiceMock = new();
        clubServiceMock.Setup(service => service.GetExistingClubIdentifiers()).Returns(["club"]);
        clubServiceMock
            .Setup(service => service.ClubSignInAsync("club", "password", It.IsAny<Func<int, int, Task<bool>>>() ))
            .Returns(() =>
            {
                signInStarted.Set();
                allowSignInToFinish.Wait(TimeSpan.FromSeconds(5));
                return Task.FromResult<ClubPreferences?>(null);
            });
        Mock<IPageNavigationService> navigationServiceMock = new();
        Mock<IMessageBoxService> messageBoxServiceMock = new();
        StartViewModel viewModel = CreateViewModel(clubServiceMock, navigationServiceMock, messageBoxServiceMock);
        viewModel.SelectedClub = "club";
        viewModel.Password = "password";

        viewModel.SignInCommand.Execute(null);

        Assert.True(signInStarted.Wait(TimeSpan.FromSeconds(1)));
        Assert.True(viewModel.IsSigningIn);
        Assert.Equal(LocalizationHelper.GetString("SigningInButton"), viewModel.SignInButtonText);

        allowSignInToFinish.Set();
        await WaitForSignInToFinishAsync(viewModel);
    }

    [Fact]
    public async Task SignInCommand_WhenSchemaIsNewerThanSupported_ShowsLocalizedVersionError()
    {
        Mock<IClubService> clubServiceMock = new();
        clubServiceMock.Setup(service => service.GetExistingClubIdentifiers()).Returns(["club"]);
        clubServiceMock
            .Setup(service => service.ClubSignInAsync("club", "password", It.IsAny<Func<int, int, Task<bool>>>() ))
            .ThrowsAsync(new UnsupportedDatabaseSchemaVersionException(4, 3));
        Mock<IPageNavigationService> navigationServiceMock = new();
        Mock<IMessageBoxService> messageBoxServiceMock = new();
        string? errorMessage = null;
        messageBoxServiceMock
            .Setup(service => service.ShowError(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((message, _) => errorMessage = message)
            .Returns(CustomMessageBoxResult.OK);
        StartViewModel viewModel = CreateViewModel(clubServiceMock, navigationServiceMock, messageBoxServiceMock);
        viewModel.SelectedClub = "club";
        viewModel.Password = "password";

        viewModel.SignInCommand.Execute(null);
        await WaitForSignInToFinishAsync(viewModel);

        Assert.NotNull(errorMessage);
        Assert.Contains("4", errorMessage);
        Assert.Contains("3", errorMessage);
        Assert.Empty(viewModel.Password);
        messageBoxServiceMock.Verify(service => service.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        navigationServiceMock.Verify(service => service.HardNavigateTo<DashboardViewModel>(), Times.Never);
    }

    [Fact]
    public async Task SignInCommand_WhenMigrationIsDeclined_DoesNotSignInOrShowCredentialsError()
    {
        Mock<IClubService> clubServiceMock = new();
        clubServiceMock.Setup(service => service.GetExistingClubIdentifiers()).Returns(["club"]);
        clubServiceMock
            .Setup(service => service.ClubSignInAsync("club", "password", It.IsAny<Func<int, int, Task<bool>>>() ))
            .Returns(async (string _, string _, Func<int, int, Task<bool>> confirmMigrationAsync) =>
                await confirmMigrationAsync(2, 3) ? new ClubPreferences() : null);
        Mock<IPageNavigationService> navigationServiceMock = new();
        Mock<IMessageBoxService> messageBoxServiceMock = new();
        string? confirmationMessage = null;
        messageBoxServiceMock
            .Setup(service => service.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((message, _) => confirmationMessage = message)
            .Returns(CustomMessageBoxResult.No);
        StartViewModel viewModel = CreateViewModel(clubServiceMock, navigationServiceMock, messageBoxServiceMock);
        viewModel.SelectedClub = "club";
        viewModel.Password = "password";

        viewModel.SignInCommand.Execute(null);
        await WaitForSignInToFinishAsync(viewModel);

        Assert.NotNull(confirmationMessage);
        Assert.Contains("2", confirmationMessage);
        Assert.Contains("3", confirmationMessage);
        Assert.Empty(viewModel.Password);
        navigationServiceMock.Verify(service => service.HardNavigateTo<DashboardViewModel>(), Times.Never);
        messageBoxServiceMock.Verify(service => service.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignInCommand_WhenMigrationIsConfirmed_NavigatesToDashboard()
    {
        ClubPreferences preferences = new();
        Mock<IClubService> clubServiceMock = new();
        clubServiceMock.Setup(service => service.GetExistingClubIdentifiers()).Returns(["club"]);
        clubServiceMock
            .Setup(service => service.ClubSignInAsync("club", "password", It.IsAny<Func<int, int, Task<bool>>>() ))
            .Returns(async (string _, string _, Func<int, int, Task<bool>> confirmMigrationAsync) =>
                await confirmMigrationAsync(2, 3) ? preferences : null);
        Mock<IPageNavigationService> navigationServiceMock = new();
        Mock<IMessageBoxService> messageBoxServiceMock = new();
        messageBoxServiceMock
            .Setup(service => service.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(CustomMessageBoxResult.Yes);
        StartViewModel viewModel = CreateViewModel(clubServiceMock, navigationServiceMock, messageBoxServiceMock);
        viewModel.SelectedClub = "club";
        viewModel.Password = "password";

        viewModel.SignInCommand.Execute(null);
        await WaitForSignInToFinishAsync(viewModel);

        Assert.Empty(viewModel.Password);
        navigationServiceMock.Verify(service => service.HardNavigateTo<DashboardViewModel>(), Times.Once);
        messageBoxServiceMock.Verify(service => service.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private static StartViewModel CreateViewModel(
        Mock<IClubService> clubServiceMock,
        Mock<IPageNavigationService> navigationServiceMock,
        Mock<IMessageBoxService> messageBoxServiceMock)
    {
        return new StartViewModel(
            NullLogger.Instance,
            navigationServiceMock.Object,
            clubServiceMock.Object,
            messageBoxServiceMock.Object);
    }

    private static async Task WaitForSignInToFinishAsync(StartViewModel viewModel)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        while (viewModel.IsSigningIn)
        {
            await Task.Delay(10, timeout.Token);
        }
    }
}
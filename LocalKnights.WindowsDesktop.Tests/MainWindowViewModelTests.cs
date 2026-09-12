using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Unit tests for <see cref="MainWindowViewModel"/>.
/// </summary>
public sealed class MainWindowViewModelTests
{
    [Fact]
    public void DataAccessStateChanged_UpdatesAndNotifiesShowDataAccess()
    {
        bool isReading = false;
        Mock<IDatabaseService> databaseServiceMock = new();
        databaseServiceMock.SetupGet(service => service.IsReadOperationInProgress).Returns(() => isReading);
        databaseServiceMock.SetupGet(service => service.IsWriteOperationInProgress).Returns(false);
        UISettings settings = new UISettings();
        Mock<IUISettingsService> uiSettingsServiceMock = new();
        uiSettingsServiceMock.SetupGet(service => service.CurrentAppSettings).Returns(settings);
        uiSettingsServiceMock.Setup(service => service.Load(It.IsAny<bool>())).Returns(settings);
        MainWindowViewModel viewModel = new MainWindowViewModel(
            NullLogger.Instance,
            new Mock<IPageNavigationService>().Object,
            uiSettingsServiceMock.Object,
            databaseServiceMock.Object,
            new Mock<IClubService>().Object,
            new Mock<IFileSystemService>().Object,
            new Mock<IMessageBoxService>().Object);
        List<string?> changedProperties = new();
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        isReading = true;
        databaseServiceMock.Raise(service => service.DataAccessStateChanged += null, EventArgs.Empty);

        Assert.True(viewModel.ShowDataAccess);
        Assert.Contains(nameof(MainWindowViewModel.ShowDataAccess), changedProperties);

        changedProperties.Clear();
        isReading = false;
        databaseServiceMock.Raise(service => service.DataAccessStateChanged += null, EventArgs.Empty);

        Assert.False(viewModel.ShowDataAccess);
        Assert.Contains(nameof(MainWindowViewModel.ShowDataAccess), changedProperties);
    }
}
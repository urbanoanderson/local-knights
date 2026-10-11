using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Unit tests for <see cref="CreateCompetitionViewModel"/>.
/// </summary>
public sealed class CreateCompetitionViewModelTests
{
    [Fact]
    public void PageTransitionSetup_PopulatesCompetitionMetadataFromClubDefaults()
    {
        ClubPreferences currentClub = new()
        {
            City = "Recife",
            Federation = "bra",
            DefaultChiefArbiter = "Alex Silva",
        };
        Mock<IClubService> clubServiceMock = new();
        clubServiceMock.SetupGet(service => service.CurrentClub).Returns(currentClub);

        CreateCompetitionViewModel viewModel = new(
            NullLogger.Instance,
            new Mock<IPageNavigationService>().Object,
            new Mock<ICompetitionService>().Object,
            clubServiceMock.Object,
            new Mock<IMessageBoxService>().Object,
            TimeProvider.System);

        viewModel.PageTransitionSetup(new Dictionary<string, object>());

        Assert.Equal("Recife", viewModel.City);
        Assert.Equal("BRA", viewModel.Federation);
        Assert.Equal("Alex Silva", viewModel.ChiefArbiter);
    }
}
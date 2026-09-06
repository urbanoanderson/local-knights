using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalKnights.Common.Tests.Services.Database;

/// <summary>
/// Tests club preference persistence behavior.
/// </summary>
public sealed class ClubServiceTests
{
    [Fact]
    public async Task UpdateClubAsync_WithCompetitionDefaults_PersistsValues()
    {
        ClubPreferences existingClub = new()
        {
            IdentifierName = "club",
            DisplayName = "Club",
        };
        ClubPreferences updatedClub = new()
        {
            IdentifierName = "club",
            DisplayName = "Club",
            City = "Recife",
            Federation = "BRA",
            DefaultChiefArbiter = "Alex Silva",
        };
        Mock<IDatabaseService> databaseServiceMock = new();
        databaseServiceMock
            .Setup(service => service.QuerySingleOrDefaultAsync<ClubPreferences>(It.IsAny<Func<IQueryable<ClubPreferences>, IQueryable<ClubPreferences>>>() ))
            .ReturnsAsync(existingClub);
        databaseServiceMock
            .Setup(service => service.UpdateAsync(It.IsAny<ClubPreferences>()))
            .Returns(Task.CompletedTask);
        Mock<IFileSystemService> fileSystemServiceMock = new();
        fileSystemServiceMock.Setup(service => service.GetDbFilePath("club")).Returns("club.db");
        fileSystemServiceMock.Setup(service => service.FileExists("club.db")).Returns(true);
        ClubService clubService = new(
            databaseServiceMock.Object,
            fileSystemServiceMock.Object,
            new Mock<ILogger>().Object);

        ClubPreferences? result = await clubService.UpdateClubAsync(updatedClub);

        Assert.NotNull(result);
        Assert.Equal("Recife", result.City);
        Assert.Equal("BRA", result.Federation);
        Assert.Equal("Alex Silva", result.DefaultChiefArbiter);
        databaseServiceMock.Verify(
            service => service.UpdateAsync(It.Is<ClubPreferences>(club =>
                club.City == "Recife"
                && club.Federation == "BRA"
                && club.DefaultChiefArbiter == "Alex Silva")),
            Times.Once);
    }
}
using LocalKnights.Common.Models;
using LocalKnights.Common.Helpers;
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
    public async Task ClubSignInAsync_WhenSchemaIsNewerThanSupported_ClosesConnectionAndThrows()
    {
        Mock<IDatabaseService> databaseServiceMock = new();
        databaseServiceMock.SetupGet(service => service.IsConnected).Returns(false);
        databaseServiceMock.Setup(service => service.OpenConnectionAsync("club", "password")).Returns(Task.CompletedTask);
        databaseServiceMock.Setup(service => service.GetSchemaVersionAsync()).ReturnsAsync(DatabaseMigrationHelper.SchemaVersion + 1);
        databaseServiceMock.Setup(service => service.CloseConnectionAsync()).Returns(Task.CompletedTask);
        ClubService clubService = CreateClubService(databaseServiceMock);

        UnsupportedDatabaseSchemaVersionException exception = await Assert.ThrowsAsync<UnsupportedDatabaseSchemaVersionException>(
            () => clubService.ClubSignInAsync("club", "password", (_, _) => Task.FromResult(true)));

        Assert.Equal(DatabaseMigrationHelper.SchemaVersion + 1, exception.StoredVersion);
        Assert.Equal(DatabaseMigrationHelper.SchemaVersion, exception.SupportedVersion);
        Assert.Null(clubService.CurrentClub);
        databaseServiceMock.Verify(service => service.CloseConnectionAsync(), Times.Once);
        databaseServiceMock.Verify(service => service.ApplyMigrationsAsync(), Times.Never);
    }

    [Fact]
    public async Task ClubSignInAsync_WhenMigrationIsDeclined_ClosesConnectionWithoutSigningIn()
    {
        Mock<IDatabaseService> databaseServiceMock = new();
        databaseServiceMock.SetupGet(service => service.IsConnected).Returns(false);
        databaseServiceMock.Setup(service => service.OpenConnectionAsync("club", "password")).Returns(Task.CompletedTask);
        databaseServiceMock.Setup(service => service.GetSchemaVersionAsync()).ReturnsAsync(DatabaseMigrationHelper.SchemaVersion - 1);
        databaseServiceMock.Setup(service => service.CloseConnectionAsync()).Returns(Task.CompletedTask);
        ClubService clubService = CreateClubService(databaseServiceMock);
        bool signInRaised = false;
        clubService.OnClubSignIn += _ => signInRaised = true;

        ClubPreferences? result = await clubService.ClubSignInAsync("club", "password", (_, _) => Task.FromResult(false));

        Assert.Null(result);
        Assert.Null(clubService.CurrentClub);
        Assert.False(signInRaised);
        databaseServiceMock.Verify(service => service.ApplyMigrationsAsync(), Times.Never);
        databaseServiceMock.Verify(service => service.CloseConnectionAsync(), Times.Once);
        databaseServiceMock.Verify(
            service => service.QuerySingleOrDefaultAsync<ClubPreferences>(It.IsAny<Func<IQueryable<ClubPreferences>, IQueryable<ClubPreferences>>>()),
            Times.Never);
    }

    [Fact]
    public async Task ClubSignInAsync_WhenMigrationIsConfirmed_MigratesBeforeSigningIn()
    {
        ClubPreferences preferences = new();
        Mock<IDatabaseService> databaseServiceMock = new();
        databaseServiceMock.SetupGet(service => service.IsConnected).Returns(false);
        databaseServiceMock.Setup(service => service.OpenConnectionAsync("club", "password")).Returns(Task.CompletedTask);
        databaseServiceMock.Setup(service => service.GetSchemaVersionAsync()).ReturnsAsync(DatabaseMigrationHelper.SchemaVersion - 1);
        databaseServiceMock.Setup(service => service.ApplyMigrationsAsync()).Returns(Task.CompletedTask);
        databaseServiceMock
            .Setup(service => service.QuerySingleOrDefaultAsync<ClubPreferences>(It.IsAny<Func<IQueryable<ClubPreferences>, IQueryable<ClubPreferences>>>() ))
            .ReturnsAsync(preferences);
        ClubService clubService = CreateClubService(databaseServiceMock);
        bool migrationAppliedWhenConfirmed = false;
        ClubPreferences? signedInClub = null;
        clubService.OnClubSignIn += club => signedInClub = club;

        ClubPreferences? result = await clubService.ClubSignInAsync(
            "club",
            "password",
            (storedVersion, currentVersion) =>
            {
                databaseServiceMock.Verify(service => service.ApplyMigrationsAsync(), Times.Never);
                Assert.Equal(DatabaseMigrationHelper.SchemaVersion - 1, storedVersion);
                Assert.Equal(DatabaseMigrationHelper.SchemaVersion, currentVersion);
                migrationAppliedWhenConfirmed = true;
                return Task.FromResult(true);
            });

        Assert.True(migrationAppliedWhenConfirmed);
        Assert.Same(preferences, result);
        Assert.Same(preferences, signedInClub);
        Assert.Equal("club", result.IdentifierName);
        databaseServiceMock.Verify(service => service.ApplyMigrationsAsync(), Times.Once);
        databaseServiceMock.Verify(service => service.CloseConnectionAsync(), Times.Never);
    }

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

    private static ClubService CreateClubService(Mock<IDatabaseService> databaseServiceMock)
    {
        Mock<IFileSystemService> fileSystemServiceMock = new();
        fileSystemServiceMock.Setup(service => service.GetDbFilePath("club")).Returns("club.db");
        fileSystemServiceMock.Setup(service => service.FileExists("club.db")).Returns(true);
        return new ClubService(
            databaseServiceMock.Object,
            fileSystemServiceMock.Object,
            new Mock<ILogger>().Object);
    }
}
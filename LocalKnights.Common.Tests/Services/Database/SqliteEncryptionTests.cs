using System.Text;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Tests.TestUtils;
using Microsoft.Data.Sqlite;
using Moq;

namespace LocalKnights.Common.Tests.Services.Database;

public sealed class SqliteEncryptionTests : IDisposable
{
    private const string ClubIdentifier = "encrypted-club";
    private const string CorrectPassword = "correct-password";
    private readonly string _temporaryDirectory;
    private readonly string _databasePath;
    private readonly IFileSystemService _fileSystemService;

    public SqliteEncryptionTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LocalKnights-{Guid.NewGuid():N}");
        _databasePath = Path.Combine(_temporaryDirectory, $"{ClubIdentifier}.db");

        Mock<IFileSystemService> fileSystemService = new Mock<IFileSystemService>();
        fileSystemService.Setup(service => service.GetClubsDirectory()).Returns(_temporaryDirectory);
        fileSystemService.Setup(service => service.GetDbFilePath(ClubIdentifier)).Returns(_databasePath);
        fileSystemService.Setup(service => service.FileExists(_databasePath)).Returns(() => File.Exists(_databasePath));
        fileSystemService.Setup(service => service.CreateDirectory(It.IsAny<string>()))
            .Callback<string>(path => Directory.CreateDirectory(path));
        fileSystemService.Setup(service => service.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<string, string, bool>((source, destination, overwrite) => File.Copy(source, destination, overwrite));
        fileSystemService.Setup(service => service.DeleteFile(It.IsAny<string>()))
            .Callback<string>(path => File.Delete(path));
        _fileSystemService = fileSystemService.Object;
    }

    [Fact]
    public async Task OpenConnectionAsync_NewDatabase_EncryptsFileAndRequiresCorrectPassword()
    {
        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword);
            await databaseService.CloseConnectionAsync();
        }

        SqliteConnection.ClearAllPools();
        Assert.True(File.Exists(_databasePath));
        byte[] header = new byte[16];
        await using (FileStream stream = new FileStream(
            _databasePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete))
        {
            int bytesRead = await stream.ReadAsync(header);
            Assert.Equal(header.Length, bytesRead);
        }

        Assert.NotEqual("SQLite format 3\0", Encoding.ASCII.GetString(header));

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword);
            await databaseService.CloseConnectionAsync();
        }

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                databaseService.OpenConnectionAsync(ClubIdentifier, "wrong-password"));
        }
    }

    [Fact]
    public async Task VerifyPasswordAsync_WrongPassword_ThrowsInvalidOperationException()
    {
        using DatabaseService databaseService = CreateDatabaseService();
        await databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword);
        await databaseService.CloseConnectionAsync();
        ClubService clubService = new ClubService(databaseService, _fileSystemService, MockHelper.CreateMockLogger());

        await clubService.VerifyPasswordAsync(ClubIdentifier, CorrectPassword);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            clubService.VerifyPasswordAsync(ClubIdentifier, "wrong-password"));
    }

    [Fact]
    public async Task ExecuteSqlAsync_RekeyDatabase_RequiresNewPassword()
    {
        const string newPassword = "new'password";

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword);
            await databaseService.RekeyAsync(newPassword);
            await databaseService.CloseConnectionAsync();
        }

        SqliteConnection.ClearAllPools();

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword));
        }

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await databaseService.OpenConnectionAsync(ClubIdentifier, newPassword);
            await databaseService.CloseConnectionAsync();
        }
    }

    [Fact]
    public async Task UpdateClubAsync_WithPasswordChange_ReopensWithNewPasswordAndRemovesBackup()
    {
        const string newPassword = "new'password";
        string backupPath = $"{_databasePath}.password-change-backup";
        using DatabaseService databaseService = CreateDatabaseService();
        ClubService clubService = new ClubService(databaseService, _fileSystemService, MockHelper.CreateMockLogger());
        ClubPreferences preferences = new ClubPreferences
        {
            IdentifierName = ClubIdentifier,
            DisplayName = "Encrypted Club"
        };
        await clubService.CreateClubAsync(preferences, CorrectPassword);

        ClubPreferences? result = await clubService.UpdateClubAsync(preferences, CorrectPassword, newPassword);

        Assert.NotNull(result);
        Assert.True(databaseService.IsConnected);
        Assert.False(File.Exists(backupPath));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            clubService.VerifyPasswordAsync(ClubIdentifier, CorrectPassword));
        await clubService.VerifyPasswordAsync(ClubIdentifier, newPassword);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    private DatabaseService CreateDatabaseService()
    {
        return new DatabaseService(_fileSystemService, MockHelper.CreateMockLogger());
    }
}
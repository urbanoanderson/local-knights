using System.Text;
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
        fileSystemService.Setup(service => service.CreateDirectory(It.IsAny<string>()))
            .Callback<string>(path => Directory.CreateDirectory(path));
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
    public async Task ExecuteSqlAsync_RekeyDatabase_RequiresNewPassword()
    {
        const string newPassword = "new-password";

        using (DatabaseService databaseService = CreateDatabaseService())
        {
            await databaseService.OpenConnectionAsync(ClubIdentifier, CorrectPassword);
            await databaseService.ExecuteSqlAsync($"PRAGMA rekey = '{newPassword}';");
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
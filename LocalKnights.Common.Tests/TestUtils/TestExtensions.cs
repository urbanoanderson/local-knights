using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalKnights.Common.Tests.TestUtils;

public static class DatabaseServiceTestExtensions
{
    public static void OpenInMemoryDbConnection(this DatabaseService service)
    {
        SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        DbContextOptions<ChessClubContext> options = new DbContextOptionsBuilder<ChessClubContext>()
        .UseSqlite(connection)
        .Options;

        service._context = new ChessClubContext(options);
        service._context.Database.EnsureCreated();
    }
}
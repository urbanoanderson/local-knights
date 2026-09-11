using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Models;
using LocalKnights.Common.Helpers;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service for managing chess clubs, including creation, retrieval, updating, and deletion.
    /// Each club has its own encrypted SQLite database file.
    /// </summary>
    public class ClubService : IClubService
    {
        private readonly IDatabaseService _databaseService;

        private readonly IFileSystemService _fileSystemService;

        private readonly ILogger _logger;

        /// <inheritdoc/>
        public ClubPreferences? CurrentClub { get; internal set; }

        /// <inheritdoc/>
        public event Action<ClubPreferences>? OnClubSignIn;

        /// <inheritdoc/>
        public event Action? OnClubSignOut;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClubService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service for accessing club data.</param>
        /// <param name="fileSystemService">The file system service for storing club database files.</param>
        /// <param name="logger">The logger for recording service events.</param>
        public ClubService(IDatabaseService databaseService, IFileSystemService fileSystemService, ILogger logger)
        {
            _databaseService = databaseService;
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetExistingClubIdentifiers()
        {
            IEnumerable<string> existingDbFiles = _fileSystemService.GetExistingDbFiles();
            return existingDbFiles.Select(file => Path.GetFileNameWithoutExtension(file));
        }

        /// <inheritdoc/>
        public bool ClubExists(string identifierName)
        {
            string dbPath = _fileSystemService.GetDbFilePath(identifierName);
            return _fileSystemService.FileExists(dbPath);
        }

        /// <inheritdoc/>
        public async Task<ClubPreferences?> ClubSignInAsync(string identifierName, string password)
        {
            return await ClubSignInAsync(identifierName, password, (_, _) => Task.FromResult(true));
        }

        /// <inheritdoc/>
        public async Task<ClubPreferences?> ClubSignInAsync(string identifierName, string password, Func<int, int, Task<bool>> confirmMigrationAsync)
        {
            if (!ClubExists(identifierName))
                return null;

            try
            {
                if (_databaseService.IsConnected)
                {
                    throw new InvalidOperationException("A club is already signed in. Please sign out before signing in to another club.");
                }

                await _databaseService.OpenConnectionAsync(identifierName, password);

                int storedSchemaVersion = await _databaseService.GetSchemaVersionAsync();
                if (storedSchemaVersion > DatabaseMigrationHelper.SchemaVersion)
                {
                    throw new UnsupportedDatabaseSchemaVersionException(storedSchemaVersion, DatabaseMigrationHelper.SchemaVersion);
                }

                if (storedSchemaVersion < DatabaseMigrationHelper.SchemaVersion)
                {
                    bool migrationConfirmed = await confirmMigrationAsync(storedSchemaVersion, DatabaseMigrationHelper.SchemaVersion);
                    if (!migrationConfirmed)
                    {
                        await _databaseService.CloseConnectionAsync();
                        return null;
                    }

                    await _databaseService.ApplyMigrationsAsync();
                }

                this.CurrentClub = await _databaseService.QuerySingleOrDefaultAsync<ClubPreferences>(query => query);
                this.CurrentClub!.IdentifierName = identifierName; // Ensure the identifier is set

                if (CurrentClub != null)
                {
                    OnClubSignIn?.Invoke(this.CurrentClub);
                }

                return CurrentClub;
            }
            catch (UnsupportedDatabaseSchemaVersionException)
            {
                await _databaseService.CloseConnectionAsync();
                this.CurrentClub = null;
                throw;
            }
            catch (Exception ex)
            {
                await _databaseService.CloseConnectionAsync();
                this.CurrentClub = null;
                _logger.LogError(ex, $"Failed to open club database for identifier '{identifierName}'. Possible invalid password or corrupted database. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task ClubSignOutAsync()
        {
            if (_databaseService.IsConnected)
            {
                await _databaseService.CloseConnectionAsync();
            }

            if (CurrentClub != null)
            {
                CurrentClub = null;
                OnClubSignOut?.Invoke();
            }
        }

        /// <inheritdoc/>
        public async Task<ClubPreferences> CreateClubAsync(ClubPreferences clubPreferences, string password)
        {
            if (ClubExists(clubPreferences.IdentifierName))
                throw new InvalidOperationException("Club with this identifier already exists");

            // Open connection with password to create encrypted database
            await _databaseService.OpenConnectionAsync(clubPreferences.IdentifierName, password);

            // Ensure database is created
            await _databaseService.AddAsync(clubPreferences);

            this.CurrentClub = clubPreferences;
            OnClubSignIn?.Invoke(this.CurrentClub);

            return clubPreferences;
        }

        /// <inheritdoc/>
        public async Task<ClubPreferences?> UpdateClubAsync(ClubPreferences updatedClubPreferences, string? newPassword = null)
        {
            if (!ClubExists(updatedClubPreferences.IdentifierName))
                return null;

            ClubPreferences? existingClubPreferences = await _databaseService.QuerySingleOrDefaultAsync<ClubPreferences>(query => query);

            if (existingClubPreferences == null)
                return null;

            existingClubPreferences.DisplayName = updatedClubPreferences.DisplayName;
            existingClubPreferences.Description = updatedClubPreferences.Description;
            existingClubPreferences.City = updatedClubPreferences.City;
            existingClubPreferences.Federation = updatedClubPreferences.Federation;
            existingClubPreferences.DefaultChiefArbiter = updatedClubPreferences.DefaultChiefArbiter;
            existingClubPreferences.SocialMediaLinks = updatedClubPreferences.SocialMediaLinks;
            existingClubPreferences.RatingSystem = updatedClubPreferences.RatingSystem;
            existingClubPreferences.Rating = updatedClubPreferences.Rating;
            existingClubPreferences.MinRating = updatedClubPreferences.MinRating;
            existingClubPreferences.MaxRating = updatedClubPreferences.MaxRating;
            existingClubPreferences.Deviation = updatedClubPreferences.Deviation;
            existingClubPreferences.Volatility = updatedClubPreferences.Volatility;
            existingClubPreferences.ProvisionalGames = updatedClubPreferences.ProvisionalGames;
            existingClubPreferences.UpdatedAt = updatedClubPreferences.UpdatedAt;

            await _databaseService.UpdateAsync(existingClubPreferences);
            this.CurrentClub = existingClubPreferences;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                await ChangePasswordAsync(updatedClubPreferences.IdentifierName, newPassword);
            }

            return existingClubPreferences;
        }

        /// <inheritdoc/>
        public async Task VerifyPasswordAsync(string identifierName, string password)
        {
            if (!ClubExists(identifierName))
            {
                throw new InvalidOperationException("Club does not exist");
            }

            string dbPath = _fileSystemService.GetDbFilePath(identifierName);
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Password = password,
            };

            await using SqliteConnection connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            try
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify password for club '{identifierName}'. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                throw new InvalidOperationException("Invalid password");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteClubAsync(string identifierName, string password)
        {
            if (!ClubExists(identifierName))
                return;

            bool wasCurrentClub = string.Equals(CurrentClub?.IdentifierName, identifierName, StringComparison.Ordinal);

            if (wasCurrentClub)
            {
                await ClubSignOutAsync();
            }

            // Verify password by trying to open the database
            try
            {
                await _databaseService.OpenConnectionAsync(identifierName, password);
                await _databaseService.CloseConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open club database for identifier '{identifierName}'. Possible invalid password or corrupted database. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                throw new InvalidOperationException("Invalid password");
            }

            string dbPath = _fileSystemService.GetDbFilePath(identifierName);

            // Force close all SQLite connections to this database
            SqliteConnection.ClearAllPools();

            // Ensure all database connections are closed before deleting
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Wait for SQLite to fully release file locks
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Aggressive retry deletion with exponential backoff
            int maxRetries = 10;
            int delayMs = 200;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Clear pools again before each attempt
                    SqliteConnection.ClearAllPools();

                    // Try to delete the file
                    _fileSystemService.DeleteFile(dbPath);
                    return;
                }
                catch (IOException) when (attempt < maxRetries - 1)
                {
                    // Force garbage collection and wait
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 5000); // Exponential backoff, max 5 seconds
                }
                catch (UnauthorizedAccessException) when (attempt < maxRetries - 1)
                {
                    // Force garbage collection and wait
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 5000); // Exponential backoff, max 5 seconds
                }
            }

            // If all retries failed, throw the final exception
            throw new IOException($"Unable to delete database file {dbPath} after {maxRetries} attempts. The file may be locked by another process.");
        }

        /// <summary>
        /// Changes the password of the club's database by executing the PRAGMA rekey command.
        /// This method assumes that the database connection is already open and authenticated with the current password.
        /// </summary>
        /// <param name="clubIdentifier">The identifier of the club whose password is to be changed.</param>
        /// <param name="newPassword">The new password to set for the club's database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ChangePasswordAsync(string clubIdentifier, string newPassword)
        {
            if (!_databaseService.IsConnected)
            {
                throw new InvalidOperationException("Database connection is not open");
            }

            try
            {
                //Escape Sql Literal
                string escapedPassword = newPassword.Replace("'", "''");
                string pragmaCommand = $"PRAGMA rekey = '{escapedPassword}';";
                await _databaseService.ExecuteSqlAsync(pragmaCommand);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to change password: {ex.Message}", ex);
            }
        }
    }
}
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;

namespace LocalKnights.Common.Helpers
{
    /// <summary>
    /// Provides helper methods for performing database schema migrations and updates without using Entity Framework Core's built-in migration system.
    /// This is useful for applying incremental schema changes to existing databases while preserving user data,
    /// especially when the application is updated with new features that require additional columns or tables.
    /// </summary>
    public static class DatabaseMigrationHelper
    {
        /// <summary>
        /// An array mapping schema version numbers to their corresponding migration actions. Each entry in the array represents a migration step that should be applied when the stored schema version is lower than the current version. The index is the schema version number, and the value is a function that migrates it to the follow up version. This allows for incremental migrations to be applied in sequence until the database schema is fully up to date.
        /// </summary>
        private static Func<ILogger, ChessClubContext, Task>[] _migrationActions = new Func<ILogger, ChessClubContext, Task>[]
        {
            /*
            (Example) 0 to 1: Add new columns to the ClubPreferences table for City, Federation, and DefaultChiefArbiter.
            */
            /*async (logger, context) =>
            {
                await DatabaseMigrationHelper.AddColumnIfMissingAsync(logger, context, "ClubPreferences", "City", "TEXT", "''");
                await DatabaseMigrationHelper.AddColumnIfMissingAsync(logger, context, "ClubPreferences", "Federation", "TEXT", "''");
                await DatabaseMigrationHelper.AddColumnIfMissingAsync(logger, context, "ClubPreferences", "DefaultChiefArbiter", "TEXT", "''");
            },*/

            // Add future migrations here as needed.
            // ...
        };

        /// <summary>
        /// The current schema version of the Database entities. This version number is automatically  incremented whenever a schema migration action is added to the migration array.
        /// </summary>
        public static int SchemaVersion = _migrationActions.Length;

        /// <summary>
        /// Migrates the database schema to the current version defined by <see cref="SchemaVersion"/>. It checks the stored schema version in the ClubPreferences row and applies any necessary migration actions in sequence until the database is up to date. If the stored version is higher than the current version, an exception is thrown, indicating that the database was created with a newer version of the application.
        /// </summary>
        /// <param name="logger">The logger instance for logging migration actions and errors.</param>
        /// <param name="context">The database context.</param>
        public static async Task MigrateDatabaseSchema(ILogger logger, ChessClubContext context)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            ClubPreferences? preferences = await context.ClubPreferences.FirstOrDefaultAsync();

            ArgumentNullException.ThrowIfNull(preferences, "ClubPreferences row is missing in the database.");

            if (preferences.SchemaVersion > SchemaVersion)
            {
                throw new ArgumentException("Database schema version is higher than the application schema version. This indicates that the database was created with a newer version of the application.", nameof(preferences));
            }

            while (preferences.SchemaVersion < SchemaVersion)
            {
                int currentVersion = preferences.SchemaVersion;

                if (_migrationActions.Length > currentVersion && _migrationActions[currentVersion] != null)
                {
                    var actionsToPerform = _migrationActions[currentVersion];
                    await actionsToPerform(logger, context);
                }
                else
                {
                    logger.LogWarning($"No migration action defined for schema version {currentVersion}. Skipping migration actions for this version.");
                }

                preferences.SchemaVersion++;
                preferences.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                logger.LogInformation($"Database schema migrated from version {currentVersion} to {preferences.SchemaVersion}.");
            }
        }

        /// <summary>
        /// Adds a new column to a specified table if it does not already exist.
        /// This method checks for the existence of the column before attempting to add it,
        /// preventing errors from trying to add a column that already exists.
        /// It uses raw SQL commands to perform the schema alteration,
        /// which allows for more direct control over the database structure without relying on Entity Framework Core's migration system.
        /// If a default value is provided, it will be set for existing rows where the column is NULL after adding the column.
        /// </summary>
        /// <param name="_logger">The logger instance for logging migration actions and errors.</param>
        /// <param name="context">The database context.</param>
        /// <param name="tableName">The name of the table to which to add the column.</param>
        /// <param name="columnName">The name of the column to add.</param>
        /// <param name="columnType">The data type of the column to add.</param>
        /// <param name="defaultValue">Optional default value to set for existing rows. If provided, existing NULL values in the new column will be updated to this value.</param>
        /// <returns>A task representing the asynchronous operation with a boolean indicating column was added.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task<bool> AddColumnIfMissingAsync(ILogger _logger, ChessClubContext context, string tableName, string columnName, string columnType, string? defaultValue = null)
        {
            if (!await ColumnExistsAsync(context, tableName, columnName))
            {
                if (context == null)
                {
                    return false;
                }

                using System.Data.Common.DbCommand command = context.Database.GetDbConnection().CreateCommand();
                string alterSql = $"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] {columnType}";

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    alterSql += $" DEFAULT {defaultValue}";
                }

                alterSql += ";";
                command.CommandText = alterSql;
                System.Data.Common.DbConnection connection = command.Connection ?? throw new InvalidOperationException("Database connection is not available.");

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await command.ExecuteNonQueryAsync();

                // If a default value was provided, update existing rows that are NULL
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    string updateSql = $"UPDATE [{tableName}] SET [{columnName}] = {defaultValue} WHERE [{columnName}] IS NULL;";
                    using System.Data.Common.DbCommand updateCommand = context.Database.GetDbConnection().CreateCommand();
                    updateCommand.CommandText = updateSql;
                    await updateCommand.ExecuteNonQueryAsync();
                }

                _logger.LogInformation($"Database schema updated to include '{columnName}' column in '{tableName}' table.");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a specific column exists in a given table within the database.
        /// This method executes a PRAGMA table_info command to retrieve the schema information of the specified table and iterates through the results to determine if the column is present.
        /// It returns true if the column exists, and false otherwise.
        /// This is a crucial step before attempting to add a new column to ensure that we do not try to add a column that already exists, which would result in an error.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="columnName">The name of the column to check.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task<bool> ColumnExistsAsync(ChessClubContext context, string tableName, string columnName)
        {
            if (context == null)
            {
                return false;
            }

            using System.Data.Common.DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"PRAGMA table_info([{tableName}]);";
            System.Data.Common.DbConnection connection = command.Connection ?? throw new InvalidOperationException("Database connection is not available.");

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader.GetString(reader.GetOrdinal("name")).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

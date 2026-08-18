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
    /// The methods in this class can be called during application startup to ensure the database schema is up to date before the application fully initializes.
    /// </summary>
    public static class DatabaseMigrationHelper
    {
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
        public static async Task<bool> AddColumnIfMissingAsync(ILogger _logger, ChessClubContext context, string tableName, string columnName, string columnType, string? defaultValue = null)
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
        public static async Task<bool> ColumnExistsAsync(ChessClubContext context, string tableName, string columnName)
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

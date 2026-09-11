using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service interface for managing chess clubs.
    /// Provides methods for club creation, retrieval, updating, and deletion.
    /// </summary>
    public interface IClubService
    {
        /// <summary>
        /// Gets the currently signed-in club, or null when no club is signed in.
        /// </summary>
        ClubPreferences? CurrentClub { get; }

        /// <summary>
        /// Occurs after a club is successfully signed in.
        /// </summary>
        event Action<ClubPreferences>? OnClubSignIn;

        /// <summary>
        /// Occurs after the current club is signed out.
        /// </summary>
        event Action? OnClubSignOut;

        /// <summary>
        /// Gets the identifiers of all existing clubs in the system.
        /// </summary>
        /// <returns>An enumerable collection of club identifier names.</returns>
        IEnumerable<string> GetExistingClubIdentifiers();

        /// <summary>
        /// Determines whether a club with the specified identifier exists in the system.
        /// </summary>
        /// <param name="identifierName">The identifier name of the club to check.</param>
        /// <returns>True if the club exists; otherwise, false.</returns>
        bool ClubExists(string identifierName);

        /// <summary>
        /// Retrieves a club's preferences asynchronously using the specified identifier and password.
        /// </summary>
        /// <param name="identifierName">The identifier name of the club.</param>
        /// <param name="password">The password to decrypt the club's database file.</param>
        /// <returns>The club preferences if credentials are valid; otherwise null.</returns>
        Task<ClubPreferences?> ClubSignInAsync(string identifierName, string password);

        /// <summary>
        /// Signs in to a club after requesting approval when its database requires migration.
        /// </summary>
        /// <param name="identifierName">The identifier name of the club.</param>
        /// <param name="password">The password to decrypt the club's database file.</param>
        /// <param name="confirmMigrationAsync">Called with the stored and current schema versions when migration is required.</param>
        /// <returns>The club preferences if credentials are valid and migration is approved; otherwise null.</returns>
        Task<ClubPreferences?> ClubSignInAsync(string identifierName, string password, Func<int, int, Task<bool>> confirmMigrationAsync);

        /// <summary>
        /// Signs out the current club asynchronously and closes its database connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClubSignOutAsync();

        /// <summary>
        /// Creates a new club asynchronously with the specified identifier, preferences and password.
        /// </summary>
        /// <param name="clubPreferences">The club preferences to create.</param>
        /// <param name="password">The password to encrypt the club's database file.</param>
        /// <returns>The created club preferences.</returns>
        Task<ClubPreferences> CreateClubAsync(ClubPreferences clubPreferences, string password);

        /// <summary>
        /// Updates club's preferences asynchronously.
        /// </summary>
        /// <param name="updatedClubPreferences">The club preferences with updated information.</param>
        /// <param name="newPassword">The new password to re-encrypt the database (optional). If null, the database will not be re-encrypted.</param>
        /// <returns>The updated club preferences, or null if the club was not found.</returns>
        Task<ClubPreferences?> UpdateClubAsync(ClubPreferences updatedClubPreferences, string? newPassword = null);

        /// <summary>
        /// Deletes a club asynchronously.
        /// </summary>
        /// <param name="identifierName">The identifier name of the club to delete.</param>
        /// <param name="password">The password to verify the club deletion.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteClubAsync(string identifierName, string password);

        /// <summary>
        /// Verifies that the provided password matches the club's current password.
        /// </summary>
        /// <param name="identifierName">The identifier name of the club.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the password is incorrect or the club does not exist.</exception>
        Task VerifyPasswordAsync(string identifierName, string password);
    }
}
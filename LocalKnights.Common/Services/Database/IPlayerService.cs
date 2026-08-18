using System.Collections.Generic;
using System.Threading.Tasks;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service interface for managing players in the chess club.
    /// Provides methods for player retrieval, creation, updating, and deletion.
    /// </summary>
    public interface IPlayerService
    {
        /// <summary>
        /// Gets all players in the current club asynchronously.
        /// </summary>
        /// <returns>A list of all players in the club.</returns>
        Task<List<Player>> GetAllPlayersAsync();

        /// <summary>
        /// Gets a specific player by their identifier name asynchronously.
        /// </summary>
        /// <param name="identifierName">The identifier name of the player to retrieve.</param>
        /// <returns>The player entity if found; otherwise null.</returns>
        Task<Player?> GetPlayerByIdentifierAsync(string identifierName);

        /// <summary>
        /// Determines whether a player identifier already exists in the club.
        /// </summary>
        /// <param name="identifierName">The identifier name to check.</param>
        /// <param name="excludePlayerIdentifierName">An optional identifier to exclude from the check (useful when updating a player).</param>
        /// <returns>True if the identifier exists; otherwise false.</returns>
        Task<bool> PlayerIdentifierExistsAsync(string identifierName, string? excludePlayerIdentifierName = null);

        /// <summary>
        /// Creates a new player in the club asynchronously.
        /// </summary>
        /// <param name="player">The player entity to create.</param>
        /// <returns>The created player entity.</returns>
        Task<Player> CreatePlayerAsync(Player player);

        /// <summary>
        /// Updates an existing player asynchronously.
        /// </summary>
        /// <param name="updatedPlayer">The player entity with updated information.</param>
        /// <returns>The updated player entity, or null if the player was not found.</returns>
        Task<Player?> UpdatePlayerAsync(Player updatedPlayer);

        /// <summary>
        /// Deletes a player from the club asynchronously.
        /// </summary>
        /// <param name="identifierName">The identifier name of the player to delete.</param>
        /// <returns>True if the player was deleted; otherwise false.</returns>
        Task<bool> DeletePlayerAsync(string identifierName);

        /// <summary>
        /// Gets the ranking position of a player in the specified time format, ordered by rating.
        /// </summary>
        /// <param name="playerIdentifierName">The identifier name of the player.</param>
        /// <param name="timeFormat">The time format for which to get the position.</param>
        /// <returns>The 1-based ranking position of the player.</returns>
        int GetPlayerTimeFormatPosition(string playerIdentifierName, TimeFormat timeFormat);

        /// <summary>
        /// Gets a paginated set of players from the current club asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of players per page.</param>
        /// <param name="searchText">Optional search text to filter by display name or identifier.</param>
        /// <param name="sortParameter">The sort parameter (column and direction). If null, defaults to IdentifierName ascending.</param>
        /// <returns>A list of players for the specified page.</returns>
        Task<List<Player>> GetPlayersPagedAsync(int pageNumber, int pageSize, string? searchText = null, SortParameter? sortParameter = null);

        /// <summary>
        /// Searches players in the current club asynchronously using a free-text query.
        /// </summary>
        /// <param name="searchText">The search text to match against player names.</param>
        /// <param name="excludedPlayerIdentifierName">An optional player identifier to exclude from the results.</param>
        /// <param name="maxResults">The maximum number of results to return.</param>
        /// <returns>A list of matching players ordered by relevance.</returns>
        Task<List<Player>> SearchPlayersAsync(string searchText, string? excludedPlayerIdentifierName = null, int maxResults = 8);

        /// <summary>
        /// Gets the total count of players in the current club asynchronously.
        /// </summary>
        /// <param name="searchText">Optional search text to filter by display name or identifier.</param>
        /// <returns>The total number of players in the club.</returns>
        Task<int> GetPlayersCountAsync(string? searchText = null);

        /// <summary> Gets the count of rated games a player has played since last year for a specific time format.
        /// This is used to determine if the player's rating is provisional in the ELO system based on the club preferences. </summary>
        /// <param name="playerIdentifierName">The identifier name of the player.</param>
        /// <param name="timeFormat">The time format for which to count the rated games. If null, counts rated games for all time formats.</param>
        /// <returns>The count of rated games the player has played since last year for the specified time format.</returns>
        Task<int> GetPlayerRatedGamesCountSinceLastYear(string playerIdentifierName, TimeFormat? timeFormat);
    }
}
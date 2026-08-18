using System.Collections.Generic;
using System.Threading.Tasks;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service interface for managing chess games in the club.
    /// Provides methods for game retrieval, creation, updating, and deletion.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Checks if a game with the specified identifier exists in the current club.
        /// </summary>
        /// <param name="gameId">The identifier of the game to check.</param>
        /// <returns>True if the game exists; otherwise false.</returns>
        bool GameExists(int gameId);

        /// <summary>
        /// Gets all games in the current club asynchronously.
        /// </summary>
        /// <returns>A list of all games in the club.</returns>
        Task<List<Game>> GetAllGamesAsync();

        /// <summary>
        /// Gets a specific game by its identifier asynchronously.
        /// </summary>
        /// <param name="gameId">The identifier of the game to retrieve.</param>
        /// <returns>The game entity if found; otherwise null.</returns>
        Task<Game?> GetGameByIdAsync(int gameId);

        /// <summary>
        /// Gets all players in the current club asynchronously, ordered by rating in descending order.
        /// </summary>
        /// <returns>A list of all players in the club, ordered by rating.</returns>
        Task<List<Player>> GetAllPlayersAsync();

        /// <summary>
        /// Creates a new game asynchronously with the specified players, time format, and result.
        /// If no player can be found for either player's id, the game is still created but ratings will not be
        /// updated for the players if the game is rated (this scenario can happen in competitions).
        /// </summary>
        /// <param name="whitePlayerIdentifierName">The identifier name of the white player.</param>
        /// <param name="blackPlayerIdentifierName">The identifier name of the black player.</param>
        /// <param name="timeFormat">The time format of the game (Bullet, Blitz, Rapid, or Classical).</param>
        /// <param name="result">The result of the game (WhiteWins, BlackWins, or Draw).</param>
        /// <param name="isRated">Whether the game is rated (affects player ratings if true).</param>
        /// <param name="pgn">The Portable Game Notation containing moves and metadata (optional).</param>
        /// <returns>The created game entity.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the players are the same or the club is not found.</exception>
        Task<Game> CreateGameAsync(string whitePlayerIdentifierName,
            string blackPlayerIdentifierName,
            TimeFormat timeFormat,
            GameResult result,
            bool isRated,
            string? pgn);

        /// <summary>
        /// Updates the PGN (moves) for an existing game asynchronously.
        /// </summary>
        /// <param name="gameId">The identifier of the game to update.</param>
        /// <param name="pgn">The new Portable Game Notation (optional).</param>
        /// <returns>True if the game was updated; otherwise false.</returns>
        Task<bool> UpdateGamePgnAsync(int gameId, string? pgn);

        /// <summary>
        /// Deletes a game from the club asynchronously.
        /// </summary>
        /// <param name="gameId">The identifier of the game to delete.</param>
        /// <returns>True if the game was deleted; otherwise false.</returns>
        Task<bool> DeleteGameAsync(int gameId);

        /// <summary>
        /// Deletes multiple games from the club asynchronously. Ignores any game identifiers that do not exist in the club.
        /// </summary>
        /// <param name="gameIds">The identifiers of the games to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteGamesAsync(IEnumerable<int> gameIds);

        /// <summary>
        /// Gets a paginated set of games from the current club asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of games per page.</param>
        /// <param name="sortParameter">The sort parameter (column and direction). If null, defaults to CreatedAt descending.</param>
        /// <returns>A list of games for the specified page.</returns>
        Task<List<Game>> GetGamesPagedAsync(int pageNumber, int pageSize, SortParameter? sortParameter = null);

        /// <summary>
        /// Gets the total count of games in the current club asynchronously.
        /// </summary>
        /// <returns>The total number of games in the club.</returns>
        Task<int> GetGamesCountAsync();

        /// <summary>
        /// Gets the competition id associated with a given game, if any.
        /// </summary>
        /// <param name="gameId">Game identifier.</param>
        /// <returns>Competition id or null if not associated.</returns>
        Task<int?> GetCompetitionIdForGameAsync(int gameId);
    }
}
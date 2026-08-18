using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using System.ComponentModel;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Interface for competition-related services.
    /// </summary>
    public interface ICompetitionService
    {
        /// <summary>
        /// Retrieves all competitions.
        /// </summary>
        /// <returns>A list of all competitions.</returns>
        Task<List<Competition>> GetAllCompetitionsAsync();

        /// <summary>
        /// Retrieves a page of competitions optionally filtered by title and sorted.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="searchQuery">Optional title search query.</param>
        /// <param name="sortParameter">Optional sort configuration.</param>
        /// <returns>The requested page of competitions.</returns>
        Task<List<Competition>> GetCompetitionsPagedAsync(int pageNumber, int pageSize, string? searchQuery = null, SortParameter? sortParameter = null);

        /// <summary>
        /// Retrieves the count of competitions optionally filtered by title.
        /// </summary>
        /// <param name="searchQuery">Optional title search query.</param>
        /// <returns>The count of matching competitions.</returns>
        Task<int> GetCompetitionsCountAsync(string? searchQuery = null);

        /// <summary>
        /// Retrieves a competition by its ID.
        /// </summary>
        /// <param name="id">The competition ID.</param>
        /// <returns>The competition if found, otherwise null.</returns>
        Task<Competition?> GetCompetitionByIdAsync(int id);

        /// <summary>
        /// Creates a new competition.
        /// </summary>
        /// <param name="competition">The competition to create.</param>
        /// <returns>The created competition.</returns>
        Task<Competition> CreateCompetitionAsync(Competition competition);

        /// <summary>
        /// Updates an existing competition.
        /// </summary>
        /// <param name="competition">The competition to update.</param>
        /// <returns>The updated competition.</returns>
        Task<Competition> UpdateCompetitionAsync(Competition competition);

        /// <summary>
        /// Deletes a competition by its ID.
        /// </summary>
        /// <param name="id">The competition ID.</param>
        /// <returns>True if deleted, false otherwise.</returns>
        Task<bool> DeleteCompetitionAsync(int id);

        /// <summary>
        /// Adds a participant to a competition.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        /// <param name="playerIdentifier">The player identifier.</param>
        Task<Competition> AddParticipantAsync(Competition competition, string playerIdentifier);

        /// <summary>
        /// Removes a participant from a competition.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        /// <param name="playerIdentifier">The player identifier.</param>
        Task<Competition> RemoveParticipantAsync(Competition competition, string playerIdentifier);

        /// <summary>
        /// Starts a competition by generating the first round pairings.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        Task<Competition> StartCompetitionAsync(Competition competition);

        /// <summary>
        /// Undoes the current round in an ongoing competition.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        Task<Competition> UndoRoundAsync(Competition competition);

        /// <summary>
        /// Advances to the next round in an ongoing competition.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        Task<Competition> AdvanceRoundAsync(Competition competition);

        /// <summary>
        /// Completes a competition and calculates final standings.
        /// </summary>
        /// <param name="competition">The competition entity.</param>
        Task<Competition> CompleteCompetitionAsync(Competition competition);
    }
}
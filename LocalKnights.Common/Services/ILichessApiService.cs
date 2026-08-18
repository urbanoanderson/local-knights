namespace LocalKnights.Common.Services
{
    using LocalKnights.Common.Models;

    /// <summary>
    /// Provides access to Lichess public API endpoints used by the application.
    /// </summary>
    public interface ILichessApiService
    {
        /// <summary>
        /// Gets the available Lichess ratings for a username grouped by supported time format.
        /// </summary>
        /// <param name="username">The Lichess username to query.</param>
        /// <returns>A dictionary containing ratings for the supported time formats.</returns>
        Task<IDictionary<TimeFormat, double>> GetPlayerRatingsAsync(string username);
    }
}
namespace LocalKnights.Common.Services
{
    using LocalKnights.Common.Models;

    /// <summary>
    /// Provides access to FIDE API endpoints used by the application.
    /// </summary>
    public interface IFideApiService
    {
        /// <summary>
        /// Gets the available ratings for a FIDE username grouped by supported time format.
        /// </summary>
        /// <param name="username">The FIDE username to query.</param>
        /// <returns>A dictionary containing ratings for the supported time formats.</returns>
        Task<IDictionary<TimeFormat, double>> GetPlayerRatingsAsync(string username);
    }
}
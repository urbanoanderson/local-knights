namespace LocalKnights.Common.Services
{
    using LocalKnights.Common.Models;

    /// <summary>
    /// Provides access to chess.com public API endpoints used by the application.
    /// </summary>
    public interface IChessDotComApiService
    {
        /// <summary>
        /// Gets the avatar URL for a chess.com username when available.
        /// </summary>
        /// <param name="username">The chess.com username to query.</param>
        /// <returns>The avatar URL, or <see langword="null"/> when no avatar is available or the request fails.</returns>
        Task<string?> GetPlayerAvatarUrlAsync(string username);

        /// <summary>
        /// Downloads the avatar image bytes for a chess.com username when available.
        /// </summary>
        /// <param name="username">The chess.com username to query.</param>
        /// <returns>The avatar image bytes, or <see langword="null"/> when no avatar is available or the request fails.</returns>
        Task<byte[]?> GetPlayerAvatarImageBytesAsync(string username);

        /// <summary>
        /// Gets the available chess.com ratings for a username grouped by supported time format.
        /// </summary>
        /// <param name="username">The chess.com username to query.</param>
        /// <returns>
        /// A dictionary containing ratings for the supported time formats. Returns an empty dictionary when the user is not found,
        /// the request fails, or no supported ratings are available.
        /// </returns>
        Task<IDictionary<TimeFormat, double>> GetPlayerRatingsAsync(string username);
    }
}
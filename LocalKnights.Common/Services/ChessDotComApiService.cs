using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Default implementation for retrieving public player data from chess.com.
    /// </summary>
    public sealed class ChessDotComApiService : IChessDotComApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChessDotComApiService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChessDotComApiService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record non-fatal API failures.</param>
        public ChessDotComApiService(ILogger<ChessDotComApiService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LocalKnights", "0.9.0"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Windows", "10"));
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        }

        /// <inheritdoc/>
        public async Task<string?> GetPlayerAvatarUrlAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            try
            {
                string encodedUsername = Uri.EscapeDataString(username.Trim());
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://api.chess.com/pub/player/{encodedUsername}");
                request.Headers.Referrer = new Uri("https://www.chess.com/");

                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Chess.com avatar lookup for username '{Username}' returned status code {StatusCode}.", username, (int)response.StatusCode);
                    return null;
                }

                await using System.IO.Stream contentStream = await response.Content.ReadAsStreamAsync();
                using JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
                if (!jsonDocument.RootElement.TryGetProperty("avatar", out JsonElement avatarElement))
                {
                    return null;
                }

                string? avatarUrl = avatarElement.GetString();
                return string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Chess.com avatar lookup for username '{Username}' returned Forbidden.", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch chess.com avatar for username '{Username}'.", username);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]?> GetPlayerAvatarImageBytesAsync(string username)
        {
            string? avatarUrl = await GetPlayerAvatarUrlAsync(username);
            if (string.IsNullOrWhiteSpace(avatarUrl))
            {
                return null;
            }

            try
            {
                return await _httpClient.GetByteArrayAsync(avatarUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download chess.com avatar image for username '{Username}'.", username);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IDictionary<TimeFormat, double>> GetPlayerRatingsAsync(string username)
        {
            Dictionary<TimeFormat, double> ratings = new Dictionary<TimeFormat, double>();
            if (string.IsNullOrWhiteSpace(username))
            {
                return ratings;
            }

            try
            {
                string encodedUsername = Uri.EscapeDataString(username.Trim());
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://api.chess.com/pub/player/{encodedUsername}/stats");
                request.Headers.Referrer = new Uri("https://www.chess.com/");

                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Chess.com ratings lookup for username '{Username}' returned status code {StatusCode}.", username, (int)response.StatusCode);
                    return ratings;
                }

                await using System.IO.Stream contentStream = await response.Content.ReadAsStreamAsync();
                using JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
                JsonElement rootElement = jsonDocument.RootElement;

                TryAddRating(rootElement, "chess_bullet", TimeFormat.Bullet, ratings);
                TryAddRating(rootElement, "chess_blitz", TimeFormat.Blitz, ratings);
                TryAddRating(rootElement, "chess_rapid", TimeFormat.Rapid, ratings);
                TryAddRating(rootElement, "chess_daily", TimeFormat.Classical, ratings);

                return ratings;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Chess.com ratings lookup for username '{Username}' returned Forbidden.", username);
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch chess.com ratings for username '{Username}'.", username);
                return ratings;
            }
        }

        private static void TryAddRating(JsonElement rootElement, string propertyName, TimeFormat timeFormat, IDictionary<TimeFormat, double> ratings)
        {
            if (!rootElement.TryGetProperty(propertyName, out JsonElement statsElement))
            {
                return;
            }

            if (!statsElement.TryGetProperty("last", out JsonElement lastElement))
            {
                return;
            }

            if (!lastElement.TryGetProperty("rating", out JsonElement ratingElement))
            {
                return;
            }

            if (!ratingElement.TryGetDouble(out double rating))
            {
                return;
            }

            ratings[timeFormat] = rating;
        }
    }
}
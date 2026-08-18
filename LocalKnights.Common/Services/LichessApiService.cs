using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using LocalKnights.Common.Models;
using Microsoft.Extensions.Logging;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Default implementation for retrieving public player data from Lichess.
    /// </summary>
    public sealed class LichessApiService : ILichessApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LichessApiService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LichessApiService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record non-fatal API failures.</param>
        public LichessApiService(ILogger<LichessApiService> logger)
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
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://lichess.org/api/user/{encodedUsername}");
                request.Headers.Referrer = new Uri("https://lichess.org/");

                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Lichess ratings lookup for username '{Username}' returned status code {StatusCode}.", username, (int)response.StatusCode);
                    return ratings;
                }

                await using System.IO.Stream contentStream = await response.Content.ReadAsStreamAsync();
                using JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
                JsonElement rootElement = jsonDocument.RootElement;
                if (!rootElement.TryGetProperty("perfs", out JsonElement perfsElement))
                {
                    return ratings;
                }

                TryAddRating(perfsElement, "bullet", TimeFormat.Bullet, ratings);
                TryAddRating(perfsElement, "blitz", TimeFormat.Blitz, ratings);
                TryAddRating(perfsElement, "rapid", TimeFormat.Rapid, ratings);
                TryAddRating(perfsElement, "classical", TimeFormat.Classical, ratings);

                return ratings;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Lichess ratings lookup for username '{Username}' returned Forbidden.", username);
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Lichess ratings for username '{Username}'.", username);
                return ratings;
            }
        }

        private static void TryAddRating(JsonElement perfsElement, string propertyName, TimeFormat timeFormat, IDictionary<TimeFormat, double> ratings)
        {
            if (!perfsElement.TryGetProperty(propertyName, out JsonElement ratingGroupElement))
            {
                return;
            }

            if (!ratingGroupElement.TryGetProperty("rating", out JsonElement ratingElement))
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
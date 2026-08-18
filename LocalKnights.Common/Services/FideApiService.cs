using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Default implementation for retrieving player data from FIDE.
    /// </summary>
    public sealed class FideApiService : IFideApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FideApiService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FideApiService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record non-fatal API failures.</param>
        public FideApiService(ILogger<FideApiService> logger)
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
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://fideratings.com/api/player/{encodedUsername}/current");
                request.Headers.Referrer = new Uri("https://fideratings.com/");

                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FIDE ratings lookup for username '{Username}' returned status code {StatusCode}.", username, (int)response.StatusCode);
                    return ratings;
                }

                await using System.IO.Stream contentStream = await response.Content.ReadAsStreamAsync();
                using JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
                JsonElement rootElement = jsonDocument.RootElement;
                if (!rootElement.TryGetProperty("current", out JsonElement currentElement))
                {
                    return ratings;
                }

                TryAddRating(currentElement, "bullet", TimeFormat.Bullet, ratings);
                TryAddRating(currentElement, "blitz", TimeFormat.Blitz, ratings);
                TryAddRating(currentElement, "rapid", TimeFormat.Rapid, ratings);
                TryAddRating(currentElement, "standard", TimeFormat.Classical, ratings);

                return ratings;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "FIDE ratings lookup for username '{Username}' returned Forbidden.", username);
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch FIDE ratings for username '{Username}'.", username);
                return ratings;
            }
        }

        private static void TryAddRating(JsonElement currentElement, string propertyName, TimeFormat timeFormat, IDictionary<TimeFormat, double> ratings)
        {
            if (!currentElement.TryGetProperty(propertyName, out JsonElement ratingGroupElement))
            {
                return;
            }

            if (!ratingGroupElement.TryGetProperty("rating", out JsonElement ratingElement))
            {
                return;
            }

            string? ratingText = ratingElement.GetString();
            if (string.IsNullOrWhiteSpace(ratingText))
            {
                return;
            }

            if (!double.TryParse(ratingText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double rating))
            {
                return;
            }

            ratings[timeFormat] = rating;
        }
    }
}
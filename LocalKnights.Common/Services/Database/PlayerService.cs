using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service for managing players in the chess club.
    /// Provides methods for player retrieval, creation, updating, and deletion with rating management.
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IClubService _clubService;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service for accessing player data.</param>
        /// <param name="clubService">The club service for accessing club-related data.</param>
        /// <param name="timeProvider">The time provider for handling time-related operations.</param>
        public PlayerService(IDatabaseService databaseService,
            IClubService clubService,
            TimeProvider timeProvider)
        {
            _databaseService = databaseService;
            _clubService = clubService;
            _timeProvider = timeProvider;
        }

        /// <inheritdoc/>
        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _databaseService.QueryAsync<Player>(query => query.OrderBy(player => player.IdentifierName));
        }

        /// <inheritdoc/>
        public async Task<Player?> GetPlayerByIdentifierAsync(string identifierName)
        {
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                return null;
            }

            List<Player> players = await _databaseService.GetWhereAsync<Player>(p => p.IdentifierName == identifierName);
            return players.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<bool> PlayerIdentifierExistsAsync(string identifierName, string? excludePlayerIdentifierName = null)
        {
            List<Player> players = await _databaseService.GetWhereAsync<Player>(p => p.IdentifierName == identifierName && (excludePlayerIdentifierName == null || p.IdentifierName != excludePlayerIdentifierName));
            return players.Any();
        }

        /// <inheritdoc/>
        public async Task<Player> CreatePlayerAsync(Player player)
        {
            player.CreatedAt = _timeProvider.GetUtcNow().DateTime;

            player.BulletRating = player.BulletRating?.Clone() ?? _clubService.CurrentClub.GetDefaultClubRating();

            player.BlitzRating = player.BlitzRating?.Clone() ?? _clubService.CurrentClub.GetDefaultClubRating();

            player.RapidRating = player.RapidRating?.Clone() ?? _clubService.CurrentClub.GetDefaultClubRating();

            player.ClassicalRating = player.ClassicalRating?.Clone() ?? _clubService.CurrentClub.GetDefaultClubRating();

            return await _databaseService.AddAsync(player);
        }

        /// <inheritdoc/>
        public async Task<Player?> UpdatePlayerAsync(Player updatedPlayer)
        {
            await _databaseService.UpdateAsync(updatedPlayer);
            return updatedPlayer;
        }

        /// <inheritdoc/>
        public async Task<bool> DeletePlayerAsync(string identifierName)
        {
            Player? player = await GetPlayerByIdentifierAsync(identifierName);

            if (player == null)
            {
                return false;
            }

            await _databaseService.DeleteAsync(player);

            return true;
        }

        /// <inheritdoc/>
        public int GetPlayerTimeFormatPosition(string playerIdentifierName, TimeFormat timeFormat)
        {
             // This is not ideal but we need to get the list of players synchronously here
            List<Player> allPlayers = _databaseService.QueryAsync<Player>(query => query.OrderBy(player => player.IdentifierName)).Result;

            var ratings = allPlayers.Select(p => {
                ChessRating playerRating = p.GetRatingForTimeFormat(timeFormat);

                return new
                {
                    p.IdentifierName,
                    Rating = playerRating.Rating,
                    Deviation = playerRating.Deviation,
                    p.CreatedAt
                };
            }).ToList();

            var sorted = ratings
                .OrderByDescending(r => r.Rating)
                .ThenBy(r => r.Deviation)
                .ThenBy(r => r.CreatedAt)
                .ToList();

            return sorted.FindIndex(r => r.IdentifierName == playerIdentifierName) + 1;
        }

        /// <inheritdoc/>
        public async Task<List<Player>> GetPlayersPagedAsync(int pageNumber, int pageSize, SortParameter? sortParameter = null)
        {
            sortParameter ??= new SortParameter(nameof(Player.IdentifierName), ListSortDirection.Ascending);
            return await GetPlayersPagedAsync(pageNumber, pageSize, null, sortParameter);
        }

        /// <inheritdoc/>
        public async Task<List<Player>> GetPlayersPagedAsync(int pageNumber, int pageSize, string? searchText = null, SortParameter? sortParameter = null)
        {
            sortParameter ??= new SortParameter(nameof(Player.IdentifierName), ListSortDirection.Ascending);
            string? normalizedSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim().ToLower();

            return await _databaseService.QueryAsync<Player>(query =>
            {
                IQueryable<Player> filteredQuery = ApplySearch(query, normalizedSearchText);
                IQueryable<Player> sortedQuery = ApplySorting(filteredQuery, sortParameter);
                int skip = (pageNumber - 1) * pageSize;
                return sortedQuery.Skip(skip).Take(pageSize);
            });
        }

        /// <inheritdoc/>
        public async Task<List<Player>> SearchPlayersAsync(string searchText, string? excludedPlayerIdentifierName = null, int maxResults = 8)
        {
            string normalizedSearchText = searchText.Trim();

            return await _databaseService.QueryAsync<Player>(query =>
            {
                IQueryable<Player> filteredQuery = query;

                if (!string.IsNullOrWhiteSpace(excludedPlayerIdentifierName))
                {
                    filteredQuery = filteredQuery.Where(player => player.IdentifierName != excludedPlayerIdentifierName);
                }

                if (!string.IsNullOrWhiteSpace(normalizedSearchText))
                {
                    string loweredSearchText = normalizedSearchText.ToLower();
                    filteredQuery = filteredQuery.Where(player =>
                        player.DisplayName.ToLower().Contains(loweredSearchText) ||
                        player.IdentifierName.ToLower().Contains(loweredSearchText));
                }

                return filteredQuery
                    .OrderBy(player => player.DisplayName)
                    .ThenBy(player => player.IdentifierName)
                    .Take(maxResults);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetPlayersCountAsync(string? searchText = null)
        {
            string? normalizedSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim().ToLower();

            return await _databaseService.CountAsync<Player>(query => ApplySearch(query, normalizedSearchText));
        }

        private static IQueryable<Player> ApplySearch(IQueryable<Player> query, string? normalizedSearchText)
        {
            if (string.IsNullOrWhiteSpace(normalizedSearchText))
            {
                return query;
            }

            return query.Where(player =>
                player.DisplayName.ToLower().Contains(normalizedSearchText) ||
                player.IdentifierName.ToLower().Contains(normalizedSearchText));
        }

        private static IQueryable<Player> ApplySorting(IQueryable<Player> query, SortParameter sortParameter)
        {
            return sortParameter.SortColumn switch
            {
                nameof(Player.IdentifierName) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.IdentifierName)
                    : query.OrderByDescending(player => player.IdentifierName),
                nameof(Player.DisplayName) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.DisplayName)
                    : query.OrderByDescending(player => player.DisplayName),
                nameof(Player.CreatedAt) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.CreatedAt)
                    : query.OrderByDescending(player => player.CreatedAt),
                nameof(Player.BulletRating) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.BulletRating.Rating)
                    : query.OrderByDescending(player => player.BulletRating.Rating),
                nameof(Player.BlitzRating) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.BlitzRating.Rating)
                    : query.OrderByDescending(player => player.BlitzRating.Rating),
                nameof(Player.RapidRating) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.RapidRating.Rating)
                    : query.OrderByDescending(player => player.RapidRating.Rating),
                nameof(Player.ClassicalRating) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.ClassicalRating.Rating)
                    : query.OrderByDescending(player => player.ClassicalRating.Rating),
                _ => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(player => player.IdentifierName)
                    : query.OrderByDescending(player => player.IdentifierName)
            };
        }

        /// <inheritdoc/>
        public async Task<int> GetPlayerRatedGamesCountSinceLastYear(string playerIdentifierName, TimeFormat? timeFormat)
        {
            DateTime oneYearAgo = _timeProvider.GetUtcNow().DateTime.AddYears(-1);

            List<Game> games = await _databaseService.GetWhereAsync<Game>(g =>
                (g.WhitePlayerId == playerIdentifierName || g.BlackPlayerId == playerIdentifierName) &&
                g.IsRated &&
                g.CreatedAt >= oneYearAgo &&
                (timeFormat == null || g.TimeFormat == timeFormat));

            return games.Count;
        }
    }
}

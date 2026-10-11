using System.ComponentModel;
using LocalKnights.Common.Helpers.ChessAlgorithms.Rating;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service for managing chess games in the club.
    /// Handles game creation with rating updates, retrieval, PGN updates, and deletion.
    /// Supports rated and unrated games with rating calculations for rated games.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IClubService _clubService;
        private readonly IPlayerService _playerService;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service for accessing game data.</param>
        /// <param name="clubService">The club service for accessing club-related data.</param>
        /// <param name="playerService">The player service for accessing and updating player ratings.</param>
        /// <param name="timeProvider">The time provider for handling time-related operations.</param>
        public GameService(IDatabaseService databaseService,
            IClubService clubService,
            IPlayerService playerService,
            TimeProvider timeProvider)
        {
            _databaseService = databaseService;
            _clubService = clubService;
            _playerService = playerService;
            _timeProvider = timeProvider;
        }

        /// <inheritdoc/>
        public bool GameExists(int gameId)
        {
            return this.GetGameByIdAsync(gameId).GetAwaiter().GetResult() != null;
        }

        /// <inheritdoc/>
        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await _databaseService.QueryAsync<Game>(query => query
                .OrderByDescending(game => game.CreatedAt)
                .ThenByDescending(game => game.Id));
        }

        /// <inheritdoc/>
        public async Task<Game?> GetGameByIdAsync(int gameId)
        {
            return await _databaseService.QuerySingleOrDefaultAsync<Game>(query => query.Where(game => game.Id == gameId));
        }

        /// <inheritdoc/>
        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _databaseService.QueryAsync<Player>(query => query
                .OrderBy(player => player.DisplayName)
                .ThenBy(player => player.IdentifierName));
        }

        /// <inheritdoc/>
        public async Task<Game> CreateGameAsync(string whitePlayerIdentifierName,
            string blackPlayerIdentifierName,
            TimeFormat timeFormat,
            GameResult result,
            bool isRated,
            string? pgn)
        {
            if (whitePlayerIdentifierName == blackPlayerIdentifierName)
                throw new InvalidOperationException("Players must be different.");

            Player? whitePlayer = await _playerService.GetPlayerByIdentifierAsync(whitePlayerIdentifierName);
            Player? blackPlayer = await _playerService.GetPlayerByIdentifierAsync(blackPlayerIdentifierName);

            ChessRating whiteOldRating = _clubService.CurrentClub.GetDefaultClubRating();
            ChessRating blackOldRating = _clubService.CurrentClub.GetDefaultClubRating();
            int whiteRatingChange = 0;
            int blackRatingChange = 0;

            if (isRated && whitePlayer != null && blackPlayer != null)
            {
                whiteOldRating = whitePlayer.GetRatingForTimeFormat(timeFormat).Clone();
                blackOldRating = blackPlayer.GetRatingForTimeFormat(timeFormat).Clone();
                int whiteRatedGamesPlayerPlayedLastYear = 0;
                int blackRatedGamesPlayerPlayedLastYear = 0;

                if (_clubService.CurrentClub.RatingSystem == RatingSystem.ELO)
                {
                    whiteRatedGamesPlayerPlayedLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(whitePlayerIdentifierName, timeFormat);
                    blackRatedGamesPlayerPlayedLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(blackPlayerIdentifierName, timeFormat);
                }

                ChessRating whiteNewRating;
                ChessRating blackNewRating;

                // Calculate updated ratings
                if (_clubService.CurrentClub.RatingSystem == RatingSystem.ELO)
                {
                    int whiteKFactor = EloAlgorithm.GetKFactorBasedOnNumberOfGames(whiteRatedGamesPlayerPlayedLastYear, _clubService.CurrentClub.ProvisionalGames, whiteOldRating);
                    int blackKFactor = EloAlgorithm.GetKFactorBasedOnNumberOfGames(blackRatedGamesPlayerPlayedLastYear, _clubService.CurrentClub.ProvisionalGames, blackOldRating);
                    (whiteNewRating, blackNewRating) = EloAlgorithm.CalculateNewRatings(whiteOldRating, blackOldRating, result, whiteKFactor, blackKFactor);
                }
                else // Glicko-2
                {
                    (whiteNewRating, blackNewRating) = Glicko2Algorithm.CalculateNewRatings(whiteOldRating, blackOldRating, result);
                }

                whiteNewRating.Rating = Math.Clamp(whiteNewRating.Rating, _clubService.CurrentClub.MinRating, _clubService.CurrentClub.MaxRating);
                blackNewRating.Rating = Math.Clamp(blackNewRating.Rating, _clubService.CurrentClub.MinRating, _clubService.CurrentClub.MaxRating);

                // Calculate rating changes before updating
                whiteRatingChange = (int)Math.Round(whiteNewRating.Rating - whiteOldRating.Rating);
                blackRatingChange = (int)Math.Round(blackNewRating.Rating - blackOldRating.Rating);

                whitePlayer.SetRatingForTimeFormat(timeFormat, whiteNewRating);
                blackPlayer.SetRatingForTimeFormat(timeFormat, blackNewRating);

                await _databaseService.UpdateAsync(whitePlayer);
                await _databaseService.UpdateAsync(blackPlayer);
            }

            Game game = new Game
            {
                CreatedAt = _timeProvider.GetUtcNow().DateTime,
                WhitePlayerId = whitePlayerIdentifierName,
                BlackPlayerId = blackPlayerIdentifierName,
                TimeFormat = timeFormat,
                Result = result,
                IsRated = isRated,
                WhitePlayerRating = whiteOldRating,
                BlackPlayerRating = blackOldRating,
                WhitePlayerRatingChange = whiteRatingChange,
                BlackPlayerRatingChange = blackRatingChange,
                Pgn = string.IsNullOrWhiteSpace(pgn) ? null : pgn,
            };

            game = await _databaseService.AddAsync(game);

            // Return the game with players loaded
            return await _databaseService.GetByIdAsync<Game>(game.Id);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateGamePgnAsync(int gameId, string? pgn)
        {
            Game? game = await _databaseService.GetByIdAsync<Game>(gameId);

            if (game == null)
            {
                return false;
            }

            game.Pgn = string.IsNullOrWhiteSpace(pgn) ? null : pgn;
            await _databaseService.UpdateAsync(game);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteGameAsync(int gameId)
        {
            Game? game = await _databaseService.GetByIdAsync<Game>(gameId);

            if (game == null)
            {
                return false;
            }

            await _databaseService.DeleteAsync(game);
            return true;
        }

        /// <inheritdoc/>
        public async Task DeleteGamesAsync(IEnumerable<int> gameIds)
        {
            var gamesToDelete = await _databaseService.GetWhereAsync<Game>(game => gameIds.Contains(game.Id));
            await _databaseService.DeleteRangeAsync(gamesToDelete);
        }

        /// <inheritdoc/>
        public async Task<List<Game>> GetGamesPagedAsync(int pageNumber, int pageSize, SortParameter? sortParameter = null)
        {
            sortParameter ??= new SortParameter(nameof(Game.CreatedAt), ListSortDirection.Descending);

            return await _databaseService.QueryAsync<Game>(query =>
            {
                IQueryable<Game> sortedQuery = ApplySorting(query, sortParameter);
                int skip = (pageNumber - 1) * pageSize;
                return sortedQuery.Skip(skip).Take(pageSize);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetGamesCountAsync()
        {
            return await _databaseService.CountAsync<Game>();
        }

        private static IQueryable<Game> ApplySorting(IQueryable<Game> query, SortParameter sortParameter)
        {
            return sortParameter.SortColumn switch
            {
                nameof(Game.Id) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.Id)
                    : query.OrderByDescending(game => game.Id),
                nameof(Game.CreatedAt) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.CreatedAt)
                    : query.OrderByDescending(game => game.CreatedAt),
                nameof(Game.TimeFormat) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.TimeFormat)
                    : query.OrderByDescending(game => game.TimeFormat),
                nameof(Game.Result) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.Result)
                    : query.OrderByDescending(game => game.Result),
                nameof(Game.IsRated) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.IsRated)
                    : query.OrderByDescending(game => game.IsRated),
                _ => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(game => game.CreatedAt)
                    : query.OrderByDescending(game => game.CreatedAt)
            };
        }

        /// <inheritdoc/>
        public async Task<int?> GetCompetitionIdForGameAsync(int gameId)
        {
            // Use the database service to find a pairing for this game
            var pairings = await _databaseService.GetWhereAsync<CompetitionPairing>(cp => cp.GameId == gameId);
            var pairing = pairings.FirstOrDefault();
            return pairing?.CompetitionId;
        }
    }
}

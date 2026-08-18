using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service for managing competitions.
    /// </summary>
    public class CompetitionService : ICompetitionService
    {
        private readonly ILogger _logger;
        private readonly IDatabaseService _databaseService;
        private readonly IClubService _clubService;
        private readonly IPlayerService _playerService;
        private readonly IGameService _gameService;
        private readonly IRandomService _randomService;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionService"/> class.
        /// </summary>
        /// <param name="logger">The application logger.</param>
        /// <param name="databaseService">The database service.</param>
        /// <param name="clubService">The club service.</param>
        /// <param name="playerService">The player service.</param>
        /// <param name="gameService">The game service.</param>
        /// <param name="randomService">The random service.</param>
        /// <param name="timeProvider">The time provider.</param>
        public CompetitionService(ILogger logger,
            IDatabaseService databaseService,
            IClubService clubService,
            IPlayerService playerService,
            IGameService gameService,
            IRandomService randomService,
            TimeProvider timeProvider)
        {
            _logger = logger;
            _databaseService = databaseService;
            _clubService = clubService;
            _playerService = playerService;
            _gameService = gameService;
            _randomService = randomService;
            _timeProvider = timeProvider;
        }

        /// <inheritdoc/>
        public async Task<List<Competition>> GetAllCompetitionsAsync()
        {
            List<Competition> competitions = await _databaseService.QueryAsync<Competition>(query => query
                .Include(competition => competition.CompetitionParticipants)
                .OrderByDescending(competition => competition.CreatedAt));

            return competitions;
        }

        /// <inheritdoc/>
        public async Task<List<Competition>> GetCompetitionsPagedAsync(int pageNumber, int pageSize, string? searchQuery = null, SortParameter? sortParameter = null)
        {
            sortParameter ??= new SortParameter(nameof(Competition.CreatedAt), ListSortDirection.Descending);
            int skip = Math.Max(0, (pageNumber - 1) * pageSize);
            string? normalizedSearchQuery = string.IsNullOrWhiteSpace(searchQuery) ? null : searchQuery.Trim().ToLower();

            return await _databaseService.QueryAsync<Competition>(query =>
            {
                IQueryable<Competition> filteredQuery = query.Include(competition => competition.CompetitionParticipants);

                if (!string.IsNullOrWhiteSpace(normalizedSearchQuery))
                {
                    filteredQuery = filteredQuery.Where(competition => competition.Title.ToLower().Contains(normalizedSearchQuery));
                }

                IQueryable<Competition> sortedQuery = ApplySorting(filteredQuery, sortParameter);
                return sortedQuery.Skip(skip).Take(pageSize);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetCompetitionsCountAsync(string? searchQuery = null)
        {
            string? normalizedSearchQuery = string.IsNullOrWhiteSpace(searchQuery) ? null : searchQuery.Trim().ToLower();

            return await _databaseService.CountAsync<Competition>(query =>
            {
                if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
                {
                    return query;
                }

                return query.Where(competition => competition.Title.ToLower().Contains(normalizedSearchQuery));
            });
        }

        /// <inheritdoc/>
        public async Task<Competition?> GetCompetitionByIdAsync(int id)
        {
            Competition? competition = await _databaseService.QuerySingleOrDefaultAsync<Competition>(query => query
                .Include(competition => competition.CompetitionParticipants)
                .Include(competition => competition.CompetitionPairings)
                .Where(competition => competition.Id == id));

            if (competition != null)
            {
                competition.ComputedStandings = StandingsHelper.ComputeStandingEntries(competition);
            }

            return competition;
        }

        /// <inheritdoc/>
        public async Task<Competition> CreateCompetitionAsync(Competition competition)
        {
            await _databaseService.AddAsync(competition);
            return competition;
        }

        /// <inheritdoc/>
        public async Task<Competition> UpdateCompetitionAsync(Competition competition)
        {
            competition.ComputedStandings = StandingsHelper.ComputeStandingEntries(competition);
            await _databaseService.UpdateAsync(competition);
            return competition;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCompetitionAsync(int id)
        {
            Competition? competition = await GetCompetitionByIdAsync(id);
            if (competition == null)
            {
                return false;
            }

            await _databaseService.DeleteAsync(competition);
            return true;
        }

        /// <inheritdoc/>
        public async Task<Competition> AddParticipantAsync(Competition competition, string playerIdentifier)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            if (!competition.CompetitionParticipants.Any(cp => string.Equals(cp.ParticipantIdentifierName, playerIdentifier, StringComparison.OrdinalIgnoreCase)))
            {
                Player? player = await _playerService.GetPlayerByIdentifierAsync(playerIdentifier);

                await _databaseService.AddAsync(new CompetitionParticipant
                {
                    CompetitionId = competition.Id,
                    ParticipantIdentifierName = player?.IdentifierName ?? playerIdentifier,
                    OldRating = player?.GetRatingForTimeFormat(competition.TimeFormat).Clone() ?? _clubService.CurrentClub.GetDefaultClubRating(),
                    NewRating = null,
                });
            }

            return await GetCompetitionByIdAsync(competition.Id) ?? competition;
        }

        /// <inheritdoc/>
        public async Task<Competition> RemoveParticipantAsync(Competition competition, string playerIdentifier)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            Player? player = await _playerService.GetPlayerByIdentifierAsync(playerIdentifier);

            CompetitionParticipant? cp = competition.CompetitionParticipants
                .FirstOrDefault(p => string.Equals(p.ParticipantIdentifierName, playerIdentifier, StringComparison.OrdinalIgnoreCase));

            if (cp != null)
            {
                await _databaseService.DeleteAsync(cp);

            }

            return await GetCompetitionByIdAsync(competition.Id) ?? competition;
        }

        /// <inheritdoc/>
        public async Task<Competition> StartCompetitionAsync(Competition competition)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            if (competition.State != CompetitionState.Starting)
            {
                _logger.LogWarning("Attempted to start competition {0} but it was not in starting state.", competition.Id);
                return competition;
            }

            competition.State = CompetitionState.Ongoing;
            competition.StartedAt = _timeProvider.GetUtcNow().DateTime;
            EnsureNumberOfRounds(competition);

            return await AdvanceRoundAsync(competition);
        }

        /// <inheritdoc/>
        public async Task<Competition> UndoRoundAsync(Competition competition)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            if (competition.State != CompetitionState.Ongoing)
            {
                _logger.LogWarning("Attempted to undo round for competition {0} but it was not in ongoing state.", competition.Id);
                return competition;
            }

            List<CompetitionPairing> pairingsToDelete = competition.CompetitionPairings
                .Where(pairing => pairing.RoundNumber >= competition.CurrentRound)
                .ToList();

            List<CompetitionPairing> retainedPairings = competition.CompetitionPairings
                .Where(pairing => pairing.RoundNumber < competition.CurrentRound)
                .ToList();

            await _databaseService.DeleteRangeAsync(pairingsToDelete);

            competition.CompetitionPairings = retainedPairings;

            competition.CurrentRound = Math.Max(0, competition.CurrentRound - 1);

            if (competition.CurrentRound == 0)
            {
                competition.State = CompetitionState.Starting;
            }

            return await this.UpdateCompetitionAsync(competition);
        }

        /// <inheritdoc/>
        public async Task<Competition> AdvanceRoundAsync(Competition competition)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            if (competition.State != CompetitionState.Ongoing)
            {
                _logger.LogWarning("Attempted to advance round for competition {0} but it was not found or not on ongoing state.", competition.Id);
                return competition;
            }

            if (competition.CurrentRound >= competition.NumberOfRounds)
            {
                _logger.LogWarning("Attempted to advance round for competition {0} but it has already reached the maximum number of rounds.", competition.Id);
                return competition;
            }

            competition.CurrentRound = Math.Min(competition.CurrentRound + 1, competition.NumberOfRounds);
            IEnumerable<CompetitionPairing> currentRoundPairings = PairingHelper.GenerateCurrentRoundPairings(competition, _randomService);
            competition.CompetitionPairings = competition.CompetitionPairings.Concat(currentRoundPairings).ToList();

            await _databaseService.AddRangeAsync(currentRoundPairings);

            return await this.UpdateCompetitionAsync(competition);
        }

        /// <inheritdoc/>
        public async Task<Competition> CompleteCompetitionAsync(Competition competition)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            if (competition.State != CompetitionState.Ongoing)
            {
                _logger.LogWarning("Attempted to complete competition {0} but it was not on ongoing state.", competition.Id);
                return competition;
            }

            // Create Game entity for each pairing entity (go in the round order)
            List<CompetitionPairing> orderedPairings = competition.CompetitionPairings
                .OrderBy(pairing => pairing.RoundNumber)
                .ThenBy(pairing => pairing.TableNumber)
                .ToList();

            foreach (CompetitionPairing pairing in orderedPairings)
            {
                // Skip BYE pairings, pairings without a result, or pairings that already have a game associated
                if (pairing.IsByePairing() || pairing.Result == null || pairing.GameId != null)
                {
                    continue;
                }

                Game createdGame = await _gameService.CreateGameAsync(
                    whitePlayerIdentifierName: pairing.WhitePlayerIdentifierName,
                    blackPlayerIdentifierName: pairing.BlackPlayerIdentifierName,
                    timeFormat: competition.TimeFormat,
                    result: pairing.Result.Value,
                    isRated: competition.IsRated,
                    pgn: null);

                pairing.GameId = createdGame.Id;
                pairing.Result = createdGame.Result;
                await _databaseService.UpdateAsync(pairing);
            }

            // Update NewRating property on CompetitionParticipants if rated
            if (competition.IsRated)
            {
                foreach (CompetitionParticipant participant in competition.CompetitionParticipants)
                {
                    Player? player = await _playerService.GetPlayerByIdentifierAsync(participant.ParticipantIdentifierName);

                    if (player != null)
                    {
                        participant.NewRating = player.GetRatingForTimeFormat(competition.TimeFormat).Clone();

                        //Update competition participant entity in database
                        await _databaseService.UpdateAsync(participant);
                    }
                }
            }

            competition.ComputedStandings = StandingsHelper.ComputeStandingEntries(competition);

            // Set winner and update competition state to completed
            var winner = competition.ComputedStandings.FirstOrDefault(s => s.Classification == 1);
            competition.WinnerId = winner?.Participant.ParticipantIdentifierName;
            competition.State = CompetitionState.Completed;
            competition.CompletedAt = _timeProvider.GetUtcNow().DateTime;

            return await this.UpdateCompetitionAsync(competition);
        }

        private static void EnsureNumberOfRounds(Competition competition)
        {
            ArgumentNullException.ThrowIfNull(competition, nameof(competition));

            int participantCount = competition.CompetitionParticipants?.Count ?? 0;

            if (competition.Type == CompetitionType.SwissSystemTournament)
            {
                int maxNumberOfRounds = SwissPairingAlgorithm.CalculateMaxNumberOfRounds(participantCount);
                competition.NumberOfRounds = Math.Min(competition.NumberOfRounds, maxNumberOfRounds);
                return;
            }

            if (competition.Type == CompetitionType.RoundRobinTournament || competition.Type == CompetitionType.KnockoutTournament)
            {
                bool isDoubleRoundRobin = competition.IsDoubleRoundRobin ?? false;

                competition.NumberOfRounds = competition.Type switch
                {
                    CompetitionType.RoundRobinTournament => RoundRobinPairingAlgorithm.CalculateNumberOfRounds(participantCount, isDoubleRoundRobin),
                    CompetitionType.KnockoutTournament => KnockoutPairingAlgorithm.CalculateNumberOfRounds(participantCount),
                    _ => competition.NumberOfRounds,
                };
            }
        }

        private static IQueryable<Competition> ApplySorting(IQueryable<Competition> query, SortParameter sortParameter)
        {
            return sortParameter.SortColumn switch
            {
                nameof(Competition.Id) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.Id)
                    : query.OrderByDescending(competition => competition.Id),
                nameof(Competition.CreatedAt) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.CreatedAt)
                    : query.OrderByDescending(competition => competition.CreatedAt),
                nameof(Competition.Title) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.Title)
                    : query.OrderByDescending(competition => competition.Title),
                nameof(Competition.Type) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.Type)
                    : query.OrderByDescending(competition => competition.Type),
                nameof(Competition.TimeFormat) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.TimeFormat)
                    : query.OrderByDescending(competition => competition.TimeFormat),
                nameof(Competition.IsRated) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.IsRated)
                    : query.OrderByDescending(competition => competition.IsRated),
                nameof(Competition.State) => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.State)
                    : query.OrderByDescending(competition => competition.State),
                _ => sortParameter.SortDirection == ListSortDirection.Ascending
                    ? query.OrderBy(competition => competition.CreatedAt)
                    : query.OrderByDescending(competition => competition.CreatedAt)
            };
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers
{
    /// <summary>
    /// Provides reusable fuzzy search and matching helpers for application search features.
    /// </summary>
    public static class SearchHelper
    {
        /// <summary>
        /// Filters a list of players based on a search query using fuzzy matching.
        /// Returns players ranked by match score, with best matches first.
        /// </summary>
        /// <param name="query">The search query string.</param>
        /// <param name="players">The list of players to search within.</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited).</param>
        /// <returns>A filtered and ranked list of players matching the query.</returns>
        public static List<Player> SearchPlayers(string query, IEnumerable<Player> players, int maxResults = 0)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return players.OrderBy(player => player.DisplayName).ThenBy(player => player.IdentifierName).ToList();
            }

            List<Player> rankedPlayers = players
                .Select(player => new { Player = player, Score = ComputeMatchScore(query, player.DisplayName, player.IdentifierName) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Player.DisplayName)
                .ThenBy(x => x.Player.IdentifierName)
                .Select(x => x.Player)
                .ToList();

            if (maxResults > 0)
            {
                rankedPlayers = rankedPlayers.Take(maxResults).ToList();
            }

            return rankedPlayers;
        }

        /// <summary>
        /// Filters and ranks competitions by title using fuzzy matching.
        /// </summary>
        /// <param name="query">The search query string.</param>
        /// <param name="competitions">The competitions to search.</param>
        /// <returns>A filtered and ranked list of competitions matching the query.</returns>
        public static List<Competition> SearchCompetitionsByTitle(string query, IEnumerable<Competition> competitions)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return competitions.ToList();
            }

            List<Competition> rankedCompetitions = competitions
                .Select(competition => new { Competition = competition, Score = ComputeMatchScore(query, competition.Title) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Competition.Title)
                .Select(x => x.Competition)
                .ToList();

            return rankedCompetitions;
        }

        /// <summary>
        /// Computes a match score for a search query against one or more target strings.
        /// Higher scores indicate better matches.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="candidates">The candidate strings to evaluate.</param>
        /// <returns>A numeric score representing the quality of the match.</returns>
        public static int ComputeMatchScore(string query, params string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return 1;
            }

            string normalizedQuery = query.Trim().ToLowerInvariant();
            List<string> normalizedCandidates = candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Select(candidate => candidate.Trim().ToLowerInvariant())
                .ToList();

            if (normalizedCandidates.Count == 0)
            {
                return 0;
            }

            int score = 0;

            if (normalizedCandidates.Any(candidate => candidate == normalizedQuery))
            {
                score += 120;
            }

            if (normalizedCandidates.Any(candidate => candidate.StartsWith(normalizedQuery)))
            {
                score += 80;
            }

            if (normalizedCandidates.Any(candidate => candidate.Contains(normalizedQuery)))
            {
                score += 55;
            }

            if (normalizedCandidates.Any(candidate => candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(token => token.StartsWith(normalizedQuery))))
            {
                score += 35;
            }

            int bestDistance = normalizedCandidates.Min(candidate => ComputeLevenshteinDistance(normalizedQuery, candidate));
            if (bestDistance <= 3)
            {
                score += 30 - (bestDistance * 8);
            }

            return Math.Max(score, 0);
        }

        /// <summary>
        /// Computes the Levenshtein distance (edit distance) between two strings.
        /// Lower values indicate greater similarity.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <returns>The minimum number of single-character edits required to change one string into the other.</returns>
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            string normalizedSource = source ?? string.Empty;
            string normalizedTarget = target ?? string.Empty;

            if (normalizedSource.Length == 0)
            {
                return normalizedTarget.Length;
            }

            if (normalizedTarget.Length == 0)
            {
                return normalizedSource.Length;
            }

            int[,] matrix = new int[normalizedSource.Length + 1, normalizedTarget.Length + 1];

            for (int sourceIndex = 0; sourceIndex <= normalizedSource.Length; sourceIndex++)
            {
                matrix[sourceIndex, 0] = sourceIndex;
            }

            for (int targetIndex = 0; targetIndex <= normalizedTarget.Length; targetIndex++)
            {
                matrix[0, targetIndex] = targetIndex;
            }

            for (int sourceIndex = 1; sourceIndex <= normalizedSource.Length; sourceIndex++)
            {
                for (int targetIndex = 1; targetIndex <= normalizedTarget.Length; targetIndex++)
                {
                    int cost = normalizedSource[sourceIndex - 1] == normalizedTarget[targetIndex - 1] ? 0 : 1;
                    matrix[sourceIndex, targetIndex] = Math.Min(
                        Math.Min(matrix[sourceIndex - 1, targetIndex] + 1, matrix[sourceIndex, targetIndex - 1] + 1),
                        matrix[sourceIndex - 1, targetIndex - 1] + cost);
                }
            }

            return matrix[normalizedSource.Length, normalizedTarget.Length];
        }
    }
}
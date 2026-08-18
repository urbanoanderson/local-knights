using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;

namespace LocalKnights.Common.Helpers
{
    public static class CsvImportExportHelper
    {
        private static readonly string[] PlayerCsvColumns =
        {
            "IdentifierName",
            "DisplayName",
            "LichessUsername",
            "ChessComUsername",
            "Email",
            "Phone",
            "FideId",
            "CbxId",
            "UscfId",
            "ChessTitle",
            "BirthDate",
            "BulletRating",
            "BulletDeviation",
            "BlitzRating",
            "BlitzDeviation",
            "RapidRating",
            "RapidDeviation",
            "ClassicalRating",
            "ClassicalDeviation"
        };

        public static async Task<(List<Player> ValidPlayers, int InvalidCount)> ImportPlayersFromCsv(
            string filePath,
            IPlayerService playerService,
            ClubPreferences? clubPreferences)
        {
            var validPlayers = new List<Player>();
            int invalidCount = 0;

            using var reader = new StreamReader(filePath);
            var config = CreatePlayerCsvConfiguration();

            using var csv = new CsvReader(reader, config);
            // Try to read header; if no header, treat as invalid
            if (!await csv.ReadAsync() || !csv.ReadHeader())
            {
                return (validPlayers, 0);
            }

            while (await csv.ReadAsync())
            {
                try
                {
                    string identifier = GetTrimmedField(csv, "IdentifierName");
                    string displayName = GetTrimmedField(csv, "DisplayName");

                    if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(displayName))
                    {
                        invalidCount++;
                        continue;
                    }

                    identifier = identifier.ToLowerInvariant();
                    if (!Regex.IsMatch(identifier, "^[a-z0-9]+$") || identifier.Length > 50 || displayName.Length > 100)
                    {
                        invalidCount++;
                        continue;
                    }

                    // Check existing identifier
                    if (await playerService.PlayerIdentifierExistsAsync(identifier))
                    {
                        invalidCount++;
                        continue;
                    }

                    Player player = new Player
                    {
                        IdentifierName = identifier,
                        DisplayName = displayName,
                        LichessUsername = GetOptionalTrimmedField(csv, "LichessUsername"),
                        ChessComUsername = GetOptionalTrimmedField(csv, "ChessComUsername"),
                        Email = GetOptionalTrimmedField(csv, "Email"),
                        Phone = GetOptionalTrimmedField(csv, "Phone"),
                        FideId = GetOptionalTrimmedField(csv, "FideId"),
                        CbxId = GetOptionalTrimmedField(csv, "CbxId"),
                        UscfId = GetOptionalTrimmedField(csv, "UscfId"),
                    };

                    // ChessTitle
                    string? titleStr = GetOptionalTrimmedField(csv, "ChessTitle");
                    if (!string.IsNullOrWhiteSpace(titleStr) && Enum.TryParse<ChessTitle>(titleStr, true, out var ct))
                    {
                        player.ChessTitle = ct;
                    }

                    // BirthDate
                    string? birthDateRaw = GetOptionalTrimmedField(csv, "BirthDate");
                    if (!string.IsNullOrWhiteSpace(birthDateRaw))
                    {
                        if (DateTime.TryParse(birthDateRaw, CultureInfo.CurrentCulture, DateTimeStyles.None, out var bd))
                        {
                            if (bd.Date <= DateTime.Today)
                            {
                                player.BirthDate = bd.Date;
                            }
                            else
                            {
                                invalidCount++;
                                continue;
                            }
                        }
                        else
                        {
                            invalidCount++;
                            continue;
                        }
                    }

                    // Ratings: rating values can be provided per time format and deviation can optionally
                    // be overridden per column; volatility still follows club preferences/defaults.
                    var defaultDeviation = clubPreferences?.Deviation ?? CommonConstants.DefaultDeviation;
                    var volatility = clubPreferences?.Volatility ?? CommonConstants.DefaultVolatility;
                    double ratingLowerBound = clubPreferences?.MinRating ?? CommonConstants.DefaultMinRating;
                    double ratingUpperBound = clubPreferences?.MaxRating ?? CommonConstants.DefaultMaxRating;

                    if (TryGetNumericField(csv, "BulletRating", out double bulletRatingValue))
                    {
                        player.BulletRating = new ChessRating
                        {
                            Rating = Math.Clamp(bulletRatingValue, ratingLowerBound, ratingUpperBound),
                            Deviation = GetNumericFieldOrDefault(csv, "BulletDeviation", defaultDeviation),
                            Volatility = volatility
                        };
                    }

                    if (TryGetNumericField(csv, "BlitzRating", out double blitzRatingValue))
                    {
                        player.BlitzRating = new ChessRating
                        {
                            Rating = Math.Clamp(blitzRatingValue, ratingLowerBound, ratingUpperBound),
                            Deviation = GetNumericFieldOrDefault(csv, "BlitzDeviation", defaultDeviation),
                            Volatility = volatility
                        };
                    }

                    if (TryGetNumericField(csv, "RapidRating", out double rapidRatingValue))
                    {
                        player.RapidRating = new ChessRating
                        {
                            Rating = Math.Clamp(rapidRatingValue, ratingLowerBound, ratingUpperBound),
                            Deviation = GetNumericFieldOrDefault(csv, "RapidDeviation", defaultDeviation),
                            Volatility = volatility
                        };
                    }

                    if (TryGetNumericField(csv, "ClassicalRating", out double classicalRatingValue))
                    {
                        player.ClassicalRating = new ChessRating
                        {
                            Rating = Math.Clamp(classicalRatingValue, ratingLowerBound, ratingUpperBound),
                            Deviation = GetNumericFieldOrDefault(csv, "ClassicalDeviation", defaultDeviation),
                            Volatility = volatility
                        };
                    }

                    validPlayers.Add(player);
                }
                catch
                {
                    invalidCount++;
                    continue;
                }
            }

            return (validPlayers, invalidCount);
        }

        public static async Task ExportPlayersToCsv(string filePath, IEnumerable<Player> players)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CreatePlayerCsvConfiguration());

            foreach (string columnName in PlayerCsvColumns)
            {
                csv.WriteField(columnName);
            }

            await csv.NextRecordAsync();

            foreach (Player player in players)
            {
                foreach (string value in GetPlayerCsvValues(player))
                {
                    csv.WriteField(value);
                }

                await csv.NextRecordAsync();
            }
        }

        private static CsvConfiguration CreatePlayerCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                PrepareHeaderForMatch = args => args.Header?.Trim().ToLowerInvariant()
            };
        }

        private static string GetTrimmedField(CsvReader csv, string columnName)
        {
            return (GetOptionalTrimmedField(csv, columnName) ?? string.Empty);
        }

        private static string? GetOptionalTrimmedField(CsvReader csv, string columnName)
        {
            if (!csv.TryGetField(columnName, out string? rawValue) || string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            return rawValue.Trim();
        }

        private static IEnumerable<string> GetPlayerCsvValues(Player player)
        {
            return new[]
            {
                player.IdentifierName,
                player.DisplayName,
                player.LichessUsername ?? string.Empty,
                player.ChessComUsername ?? string.Empty,
                player.Email ?? string.Empty,
                player.Phone ?? string.Empty,
                player.FideId ?? string.Empty,
                player.CbxId ?? string.Empty,
                player.UscfId ?? string.Empty,
                player.ChessTitle == ChessTitle.None ? string.Empty : player.ChessTitle.ToString(),
                player.BirthDate?.ToString("d", CultureInfo.CurrentCulture) ?? string.Empty,
                FormatRatingValue(player.BulletRating),
                FormatDeviationValue(player.BulletRating),
                FormatRatingValue(player.BlitzRating),
                FormatDeviationValue(player.BlitzRating),
                FormatRatingValue(player.RapidRating),
                FormatDeviationValue(player.RapidRating),
                FormatRatingValue(player.ClassicalRating),
                FormatDeviationValue(player.ClassicalRating)
            };
        }

        private static string FormatRatingValue(ChessRating rating)
        {
            return rating.Rating.ToString(CultureInfo.CurrentCulture);
        }

        private static string FormatDeviationValue(ChessRating rating)
        {
            return rating.Deviation.ToString(CultureInfo.CurrentCulture);
        }

        private static bool TryGetNumericField(CsvReader csv, string columnName, out double value)
        {
            if (csv.TryGetField(columnName, out string? rawDeviation) &&
                double.TryParse(rawDeviation, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double parsedValue))
            {
                value = parsedValue;
                return true;
            }

            value = default;
            return false;
        }

        private static double GetNumericFieldOrDefault(CsvReader csv, string columnName, double fallbackValue)
        {
            return TryGetNumericField(csv, columnName, out double value)
                ? value
                : fallbackValue;
        }
    }
}

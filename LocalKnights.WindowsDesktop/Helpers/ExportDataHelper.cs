using System.Data;
using System.Globalization;
using FreeDataExportsv2;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.DisplayObjects;

namespace LocalKnights.WindowsDesktop.Helpers
{
    public static class ExportDataHelper
    {
        private static readonly DomainEnumStringConverter DomainEnumStringConverter = new DomainEnumStringConverter();

        /// <summary>
        /// Export competition details to an Excel file.
        /// This method is implemented to try to use the FreeDataExportsv2 library
        /// if available at runtime; if not available it will generate a sheet file containing
        /// the competition information, standings and rounds.
        /// </summary>
        /// <param name="competition">Competition entity to export.</param>
        /// <param name="filePath">Target file path chosen by the user.</param>
        /// <param name="standings">Standings to export</param>
        /// <param name="playerService">The player service for retrieving player information.</param>
        /// <param name="timeProvider">The time provider for handling time-related operations.</param>
        public static void ExportCompetitionToExcel(
            Competition competition,
            string filePath,
            IEnumerable<CompetitionPageStandingTableItem> standings,
            IPlayerService playerService,
            TimeProvider timeProvider)
        {
            if (competition == null) throw new ArgumentNullException(nameof(competition));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Use FreeDataExportsv2 XlsxFile to create an Excel workbook with multiple sheets.
            XlsxFile workbook = new XlsxFile
            {
                Created = timeProvider.GetUtcNow().DateTime,
                Modified = timeProvider.GetUtcNow().DateTime,
                Application = AppConstants.AppDisplayName,
                Company = AppConstants.AppDisplayName,
                Creator = AppConstants.AppDisplayName,
                LastModifiedBy = AppConstants.AppDisplayName,
            };

            // Information sheet
            string infoSheetName = LocalizationHelper.GetString("ExportInformationSheetName");

            // Header style for info sheet
            CellOptions infoHeaderStyle = new CellOptions
            {
                DataType = DataType.String,
                Bold = true,
                FontColor = "FFFFFFFF",
                BackgroundColor = "FF203864"
            };

            CellOptions infoValueOptions = new CellOptions
            {
                DataType = DataType.String,
                WrapText = true
            };

            // Information sheet
            XlsxWorksheet infoSheet = workbook.AddWorksheet(infoSheetName)
                .ApplySheetStyle(new SheetStyle { BackgroundColor = "FFF8F8F8", FontSize = 11, FontColor = "FF2C2C2C" });
            string infoField = LocalizationHelper.GetString("InfoFieldLabel");
            string infoValue = LocalizationHelper.GetString("InfoValueLabel");
            infoSheet.AddRow()
                .AddCell(infoField, infoHeaderStyle)
                .AddCell(infoValue, infoHeaderStyle);

            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("TitleLabel"), DataType.String).AddCell(competition.Title ?? string.Empty, infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("DescriptionLabel"), DataType.String).AddCell(competition.Description ?? string.Empty, infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("CityLabel"), DataType.String).AddCell(competition.City ?? string.Empty, infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("ChiefArbiterLabel"), DataType.String).AddCell(competition.ChiefArbiter ?? string.Empty, infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("FederationLabel"), DataType.String).AddCell(competition.Federation ?? string.Empty, infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("StateLabel"), DataType.String).AddCell(competition.State.ToString(), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("TypeLabel"), DataType.String).AddCell(competition.Type.ToString(), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("TimeFormatLabel"), DataType.String).AddCell(competition.TimeFormat.ToString(), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("IsRatedLabel"), DataType.String).AddCell(competition.IsRated ? LocalizationHelper.GetString("Yes") : LocalizationHelper.GetString("No"), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("NumberOfRoundsLabel"), DataType.String).AddCell(competition.NumberOfRounds.ToString(), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("CreationDateLabel"), DataType.String).AddCell(FormatDateTimeValue(competition.CreatedAt), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("StartedAtLabel"), DataType.String).AddCell(FormatDateTimeValue(competition.StartedAt), infoValueOptions);
            infoSheet.AddRow().AddCell(LocalizationHelper.GetString("CompletedAtLabel"), DataType.String).AddCell(FormatDateTimeValue(competition.CompletedAt), infoValueOptions);
            if (competition.CompetitionParticipants != null)
            {
                string participants = string.Join(", ", standings.Select(s => s.ParticipantName));
                infoSheet.AddRow().AddCell(LocalizationHelper.GetString("ParticipantsLabel"), DataType.String).AddCell(participants, infoValueOptions);
            }
            infoSheet.ColumnWidths("20", "60");
            infoSheet.TabColor = "FF203864";

            // Standings sheet
            var standingsSheetName = LocalizationHelper.GetString("ExportStandingsSheetName");
            var standingsSheet = workbook.AddWorksheet(standingsSheetName)
                .ApplySheetStyle(new SheetStyle { BackgroundColor = "FFFFFFFF", FontSize = 11, FontColor = "FF2C2C2C" });

            var headerStyle = new CellOptions
            {
                DataType = DataType.String,
                Bold = true,
                FontColor = "FFFFFFFF",
                BackgroundColor = "FF203864"
            };

            standingsSheet.AddRow()
                .AddCell(LocalizationHelper.GetString("ClassificationLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("ParticipantLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("ScoreLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("OldRatingLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("NewRatingLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("PerformanceRatingLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("BuchholzScoreLabel"), headerStyle)
                .AddCell(LocalizationHelper.GetString("SonnebornBergerScoreLabel"), headerStyle);

            foreach (CompetitionPageStandingTableItem s in standings)
            {
                standingsSheet.AddRow()
                    .AddCell(s.Classification, DataType.String)
                    .AddCell(s.ParticipantName, DataType.String)
                    .AddCell(s.Score, DataType.String)
                    .AddCell(s.OldRatingDisplay, DataType.String)
                    .AddCell(s.NewRatingDisplay, DataType.String)
                        .AddCell(s.PerformanceRatingDisplay, DataType.String)
                        .AddCell(s.BuchholzScoreDisplay, DataType.String)
                        .AddCell(s.SonnebornBergerScoreDisplay, DataType.String);
            }

                    // Column widths (chars): Classification, Participant, Score, OldRating, NewRating, PerformanceRating, Buchholz, Sonneborn-Berger
                    standingsSheet.ColumnWidths("8", "32", "8", "12", "12", "16", "14", "18");
            standingsSheet.TabColor = "FF0070C0";

            // Add one sheet per round using the full pairing history so ongoing competitions export all rounds and results.
            if (competition.CompetitionPairings != null)
            {
                var rounds = competition.CompetitionPairings
                    .GroupBy(p => p.RoundNumber)
                    .OrderBy(g => g.Key);

                foreach (var round in rounds)
                {
                    var sheetName = string.Format(LocalizationHelper.GetString("ExportRoundSheetNameFormat") ?? "Round {0}", round.Key);
                    var rSheet = workbook.AddWorksheet(sheetName)
                        .ApplySheetStyle(new SheetStyle { BackgroundColor = "FFFFFFFF", FontSize = 11, FontColor = "FF2C2C2C" });

                    rSheet.AddRow()
                        .AddCell(LocalizationHelper.GetString("TableNumberLabel") ?? "Table", headerStyle)
                        .AddCell(LocalizationHelper.GetString("WhitePlayerLabel") ?? "White", headerStyle)
                        .AddCell(LocalizationHelper.GetString("ResultLabel") ?? "Result", headerStyle)
                        .AddCell(LocalizationHelper.GetString("BlackPlayerLabel") ?? "Black", headerStyle);

                    foreach (CompetitionPairing p in round.OrderBy(p => p.TableNumber))
                    {
                        Player whitePlayer = playerService.GetPlayerByIdentifierAsync(p.WhitePlayerIdentifierName).GetAwaiter().GetResult();
                        Player blackPlayer = playerService.GetPlayerByIdentifierAsync(p.BlackPlayerIdentifierName).GetAwaiter().GetResult();
                        string whiteDisplay = whitePlayer != null ? whitePlayer.DisplayName : p.WhitePlayerIdentifierName;
                        string blackDisplay = blackPlayer != null ? blackPlayer.DisplayName : p.BlackPlayerIdentifierName;
                        string result = p.Result.HasValue ? DomainEnumStringConverter.Convert(p.Result.Value) : string.Empty;

                        // Table cell: write "BYE" for bye pairings, otherwise write table number as string
                        var tableText = string.IsNullOrWhiteSpace(p.BlackPlayerIdentifierName) ? "BYE" : p.TableNumber.ToString();

                        rSheet.AddRow()
                            .AddCell(tableText, DataType.String)
                            .AddCell(whiteDisplay, DataType.String)
                            .AddCell(result, DataType.String)
                            .AddCell(blackDisplay, DataType.String);
                    }
                    // Column widths: Table, White, Result, Black
                    rSheet.ColumnWidths("8", "30", "10", "30");
                    rSheet.TabColor = "FF00B050";
                }
            }

            // Save workbook
            workbook.Save(filePath);
        }

        private static string FormatDateTimeValue(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture)
                : string.Empty;
        }
    }
}

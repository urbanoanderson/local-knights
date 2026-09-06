using System.Globalization;
using System.Text;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;

namespace LocalKnights.WindowsDesktop.Helpers;

/// <summary>
/// Generates FIDE Tournament Report Format content.
/// </summary>
public static class ExportToTRFHelper
{
    private const char RecordTerminator = '\r';
    private const string UnknownRequiredValue = "Unknown";

    /// <summary>
    /// Converts a completed competition and its participants to TRF16 content.
    /// </summary>
    /// <param name="competition">The completed competition to export.</param>
    /// <param name="playerService">The service used to retrieve current player profile information.</param>
    /// <returns>The TRF16 content, or an empty string when the competition is not completed.</returns>
    public static async Task<string> ExportCompetitionToTrf16(
        Competition competition,
        IPlayerService playerService)
    {
        ArgumentNullException.ThrowIfNull(competition);

        if (competition.State != CompetitionState.Completed)
        {
            return string.Empty;
        }

        ArgumentNullException.ThrowIfNull(playerService);

        List<(CompetitionParticipant Participant, Player? Player)> participantData = [];
        foreach (CompetitionParticipant participant in competition.CompetitionParticipants)
        {
            Player? player = await playerService
                .GetPlayerByIdentifierAsync(participant.ParticipantIdentifierName)
                .ConfigureAwait(false);
            participantData.Add((participant, player));
        }

        List<(CompetitionParticipant Participant, Player? Player)> orderedParticipants = participantData
            .OrderByDescending(item => item.Participant.OldRating.Rating)
            .ThenBy(
                item => item.Player?.DisplayName ?? item.Participant.ParticipantIdentifierName,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
        Dictionary<string, int> startingRanks = orderedParticipants
            .Select((item, index) => new
            {
                item.Participant.ParticipantIdentifierName,
                StartingRank = index + 1,
            })
            .ToDictionary(
                item => item.ParticipantIdentifierName,
                item => item.StartingRank,
                StringComparer.OrdinalIgnoreCase);

        Trf16Tournament tournament = ConvertTournament(competition);
        List<Trf16Player> players = orderedParticipants
            .Select(item => ConvertPlayer(competition, item.Participant, item.Player, startingRanks))
            .ToList();

        return ExportCompetitionToTrf16(tournament, players);
    }

    /// <summary>
    /// Exports a tournament and its players as TRF16 content.
    /// </summary>
    /// <param name="tournament">The tournament section values.</param>
    /// <param name="players">The players and their round results.</param>
    /// <returns>The complete TRF16 file content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
    public static string ExportCompetitionToTrf16(
        Trf16Tournament tournament,
        IEnumerable<Trf16Player> players)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        ArgumentNullException.ThrowIfNull(players);

        StringBuilder builder = new StringBuilder();
        AppendTournamentSection(builder, tournament);

        foreach (Trf16Player player in players)
        {
            ArgumentNullException.ThrowIfNull(player);
            AppendPlayer(builder, player);
        }

        return builder.ToString();
    }

    private static void AppendTournamentSection(StringBuilder builder, Trf16Tournament tournament)
    {
        AppendTournamentRecord(builder, "012", tournament.TournamentName);
        AppendTournamentRecord(builder, "022", tournament.City);
        AppendTournamentRecord(builder, "032", tournament.Federation);
        AppendTournamentRecord(builder, "042", FormatDate(tournament.StartDate, "yyyy/MM/dd"));
        AppendTournamentRecord(builder, "052", FormatDate(tournament.EndDate, "yyyy/MM/dd"));
        AppendTournamentRecord(builder, "062", FormatNumber(tournament.NumberOfPlayers));
        AppendTournamentRecord(builder, "072", FormatNumber(tournament.NumberOfRatedPlayers));
        AppendTournamentRecord(builder, "082", FormatNumber(tournament.NumberOfTeams));
        AppendTournamentRecord(builder, "092", tournament.TournamentType);
        AppendTournamentRecord(builder, "102", tournament.ChiefArbiter);

        if (tournament.DeputyChiefArbiters.Count == 0)
        {
            AppendTournamentRecord(builder, "112", null);
        }
        else
        {
            foreach (string deputyChiefArbiter in tournament.DeputyChiefArbiters)
            {
                AppendTournamentRecord(builder, "112", deputyChiefArbiter);
            }
        }

        AppendTournamentRecord(builder, "122", tournament.AllottedTime);
        AppendRoundDatesRecord(builder, tournament.RoundDates);
    }

    private static Trf16Tournament ConvertTournament(Competition competition)
    {
        List<DateTime> roundDates = competition.CompletedAt.HasValue
            ? Enumerable.Repeat(competition.CompletedAt.Value, competition.NumberOfRounds).ToList()
            : [];

        return new Trf16Tournament
        {
            TournamentName = competition.Title,
            City = GetRequiredValueOrUnknown(competition.City),
            Federation = GetRequiredValueOrUnknown(competition.Federation),
            StartDate = competition.StartedAt,
            EndDate = competition.CompletedAt,
            NumberOfPlayers = competition.CompetitionParticipants.Count,
            NumberOfRatedPlayers = 0,
            NumberOfTeams = 0,
            TournamentType = FormatTournamentType(competition),
            ChiefArbiter = GetRequiredValueOrUnknown(competition.ChiefArbiter),
            DeputyChiefArbiters = [],
            AllottedTime = string.Empty,
            RoundDates = roundDates,
        };
    }

    private static string GetRequiredValueOrUnknown(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? UnknownRequiredValue : value;
    }

    private static Trf16Player ConvertPlayer(
        Competition competition,
        CompetitionParticipant participant,
        Player? player,
        IReadOnlyDictionary<string, int> startingRanks)
    {
        CompetitionStandingEntry? standing = competition.ComputedStandings?.FirstOrDefault(
            entry => string.Equals(
                entry.Participant.ParticipantIdentifierName,
                participant.ParticipantIdentifierName,
                StringComparison.OrdinalIgnoreCase));
        List<Trf16PlayerRound> rounds = Enumerable.Range(1, competition.NumberOfRounds)
            .Select(roundNumber => ConvertPlayerRound(
                competition,
                participant.ParticipantIdentifierName,
                roundNumber,
                startingRanks))
            .ToList();

        return new Trf16Player
        {
            StartingRankNumber = startingRanks[participant.ParticipantIdentifierName],
            Sex = FormatSex(player?.Gender),
            Title = FormatFideTitle(player?.ChessTitle),
            Name = player?.DisplayName ?? participant.ParticipantIdentifierName,
            FideRating = (int)Math.Round(participant.OldRating.Rating, MidpointRounding.AwayFromZero),
            FideFederation = string.Empty,
            FideNumber = player?.FideId,
            BirthDate = player?.BirthDate,
            Points = (decimal)(standing?.Score ?? 0d),
            Rank = standing?.Classification,
            Rounds = rounds,
        };
    }

    private static string FormatSex(GenderType? gender)
    {
        return gender switch
        {
            GenderType.Male => "m",
            GenderType.Female => "w",
            _ => string.Empty,
        };
    }

    private static Trf16PlayerRound ConvertPlayerRound(
        Competition competition,
        string participantIdentifierName,
        int roundNumber,
        IReadOnlyDictionary<string, int> startingRanks)
    {
        CompetitionPairing? pairing = competition.CompetitionPairings.FirstOrDefault(
            item => item.RoundNumber == roundNumber
                && (string.Equals(
                    item.WhitePlayerIdentifierName,
                    participantIdentifierName,
                    StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        item.BlackPlayerIdentifierName,
                        participantIdentifierName,
                        StringComparison.OrdinalIgnoreCase)));

        if (pairing == null)
        {
            return new Trf16PlayerRound
            {
                OpponentStartingRankNumber = 0,
                Color = "-",
                Result = "Z",
            };
        }

        if (pairing.IsByePairing())
        {
            return new Trf16PlayerRound
            {
                OpponentStartingRankNumber = 0,
                Color = "-",
                Result = FormatByeResult(competition.ByeValue),
            };
        }

        bool isWhite = string.Equals(
            pairing.WhitePlayerIdentifierName,
            participantIdentifierName,
            StringComparison.OrdinalIgnoreCase);
        string opponentIdentifierName = isWhite
            ? pairing.BlackPlayerIdentifierName
            : pairing.WhitePlayerIdentifierName;

        return new Trf16PlayerRound
        {
            OpponentStartingRankNumber = startingRanks.TryGetValue(opponentIdentifierName, out int opponentRank)
                ? opponentRank
                : 0,
            Color = isWhite ? "w" : "b",
            Result = FormatGameResult(pairing.Result, isWhite),
        };
    }

    private static string FormatTournamentType(Competition competition)
    {
        return competition.Type switch
        {
            CompetitionType.SwissSystemTournament => "Swiss-System Dutch",
            CompetitionType.KnockoutTournament => "Knockout",
            CompetitionType.RoundRobinTournament when competition.IsDoubleRoundRobin == true => "Double Round-Robin",
            CompetitionType.RoundRobinTournament => "Round-Robin",
            CompetitionType.ChessMatch => "Chess Match",
            _ => string.Empty,
        };
    }

    private static string FormatFideTitle(ChessTitle? title)
    {
        return title switch
        {
            ChessTitle.GM => "GM",
            ChessTitle.IM => "IM",
            ChessTitle.FM => "FM",
            ChessTitle.WGM => "WGM",
            ChessTitle.WIM => "WIM",
            ChessTitle.WFM => "WFM",
            ChessTitle.CM => "CM",
            ChessTitle.WCM => "WCM",
            _ => string.Empty,
        };
    }

    private static string FormatGameResult(GameResult? result, bool isWhite)
    {
        return result switch
        {
            GameResult.Draw => "=",
            GameResult.WhiteWins => isWhite ? "1" : "0",
            GameResult.BlackWins => isWhite ? "0" : "1",
            _ => string.Empty,
        };
    }

    private static string FormatByeResult(double? byeValue)
    {
        return byeValue switch
        {
            1d => "F",
            0.5d => "H",
            _ => "Z",
        };
    }

    private static void AppendTournamentRecord(StringBuilder builder, string identifier, string? value)
    {
        builder.Append(identifier);
        builder.Append(' ');
        builder.Append(Sanitize(value));
        builder.Append(RecordTerminator);
    }

    private static void AppendRoundDatesRecord(StringBuilder builder, IEnumerable<DateTime> roundDates)
    {
        builder.Append("132");

        int roundNumber = 0;
        foreach (DateTime roundDate in roundDates)
        {
            int fieldStartIndex = 91 + (roundNumber * 10);
            if (builder.Length > 0)
            {
                int currentLineLength = GetCurrentLineLength(builder);
                builder.Append(' ', fieldStartIndex - currentLineLength);
            }

            builder.Append(roundDate.ToString("yy/MM/dd", CultureInfo.InvariantCulture));
            roundNumber++;
        }

        builder.Append(RecordTerminator);
    }

    private static int GetCurrentLineLength(StringBuilder builder)
    {
        for (int index = builder.Length - 1; index >= 0; index--)
        {
            if (builder[index] == RecordTerminator)
            {
                return builder.Length - index - 1;
            }
        }

        return builder.Length;
    }

    private static void AppendPlayer(StringBuilder builder, Trf16Player player)
    {
        builder.Append("001");
        builder.Append(' ');
        builder.Append(Fit(FormatNumber(player.StartingRankNumber), 4, true));
        builder.Append(' ');
        builder.Append(Fit(player.Sex, 1));
        builder.Append(Fit(player.Title, 3));
        builder.Append(' ');
        builder.Append(Fit(player.Name, 33));
        builder.Append(' ');
        builder.Append(Fit(FormatNumber(player.FideRating), 4, true));
        builder.Append(' ');
        builder.Append(Fit(player.FideFederation, 3));
        builder.Append(' ');
        builder.Append(Fit(player.FideNumber, 11, true));
        builder.Append(' ');
        builder.Append(Fit(FormatDate(player.BirthDate, "yyyy/MM/dd"), 10));
        builder.Append(' ');
        builder.Append(Fit(FormatPoints(player.Points), 4, true));
        builder.Append(' ');
        builder.Append(Fit(FormatNumber(player.Rank), 4, true));

        foreach (Trf16PlayerRound round in player.Rounds)
        {
            builder.Append("  ");
            builder.Append(Fit(FormatOpponent(round.OpponentStartingRankNumber), 4, true));
            builder.Append(' ');
            builder.Append(Fit(round.Color, 1));
            builder.Append(' ');
            builder.Append(Fit(round.Result, 1));
        }

        builder.Append(RecordTerminator);
    }

    private static string Fit(string? value, int width, bool rightAligned = false)
    {
        string sanitizedValue = Sanitize(value);
        if (sanitizedValue.Length > width)
        {
            sanitizedValue = sanitizedValue[..width];
        }

        return rightAligned
            ? sanitizedValue.PadLeft(width)
            : sanitizedValue.PadRight(width);
    }

    private static string Sanitize(string? value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }

    private static string FormatDate(DateTime? value, string format)
    {
        return value?.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatNumber(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatPoints(decimal? value)
    {
        return value?.ToString("0.0", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatOpponent(int? opponentStartingRankNumber)
    {
        return opponentStartingRankNumber?.ToString("0000", CultureInfo.InvariantCulture) ?? "0000";
    }
}

/// <summary>
/// Models every tournament-section field defined by TRF16.
/// </summary>
public sealed class Trf16Tournament
{
    public string? TournamentName { get; set; }
    public string? City { get; set; }
    public string? Federation { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? NumberOfPlayers { get; set; }
    public int? NumberOfRatedPlayers { get; set; }
    public int? NumberOfTeams { get; set; }
    public string? TournamentType { get; set; }
    public string? ChiefArbiter { get; set; }
    public IList<string> DeputyChiefArbiters { get; set; } = new List<string>();
    public string? AllottedTime { get; set; }
    public IList<DateTime> RoundDates { get; set; } = new List<DateTime>();
}

/// <summary>
/// Models every player-section field defined by TRF16.
/// </summary>
public sealed class Trf16Player
{
    public int? StartingRankNumber { get; set; }
    public string? Sex { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public int? FideRating { get; set; }
    public string? FideFederation { get; set; }
    public string? FideNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal? Points { get; set; }
    public int? Rank { get; set; }
    public IList<Trf16PlayerRound> Rounds { get; set; } = new List<Trf16PlayerRound>();
}

/// <summary>
/// Models one player's opponent, color, and result fields for a TRF16 round.
/// </summary>
public sealed class Trf16PlayerRound
{
    public int? OpponentStartingRankNumber { get; set; }
    public string? Color { get; set; }
    public string? Result { get; set; }
}
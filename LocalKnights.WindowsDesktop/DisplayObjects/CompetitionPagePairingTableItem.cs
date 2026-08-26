using LocalKnights.Common.Models;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocalKnights.WindowsDesktop.DisplayObjects
{
    /// <summary>
    /// Represents a row in the Pairing Table of Competition Page's Rounds tab.
    /// </summary>
    public sealed class CompetitionPagePairingTableItem : INotifyPropertyChanged
    {
        private CompetitionPairing _competitionPairing;
        private Player _whitePlayer;
        private Player _blackPlayer;

        public event Action<CompetitionPairing, GameResult?>? OnSetResult;
        public event PropertyChangedEventHandler? PropertyChanged;

        public CompetitionPagePairingTableItem(
            CompetitionPairing competitionPairing,
            Player whitePlayer,
            Player blackPlayer,
            ClubPreferences? clubPreferences,
            bool canNavigateToGame)
        {
            ArgumentNullException.ThrowIfNull(competitionPairing, nameof(competitionPairing));
            ArgumentNullException.ThrowIfNull(clubPreferences, nameof(clubPreferences));

            _competitionPairing = competitionPairing;
            _whitePlayer = whitePlayer;
            _blackPlayer = blackPlayer;

            CompetitionParticipant whiteParticipant = competitionPairing.Competition.CompetitionParticipants.FirstOrDefault(p => p.ParticipantIdentifierName == competitionPairing.WhitePlayerIdentifierName);
            string whiteName = _whitePlayer == null ? whiteParticipant.ParticipantIdentifierName : _whitePlayer.DisplayName;
            string whiteRatingFormatted = FormatHelper.FormatRating(whiteParticipant.OldRating, clubPreferences.RatingSystem);
            CompetitionParticipant blackParticipant = competitionPairing.Competition.CompetitionParticipants.FirstOrDefault(p => p.ParticipantIdentifierName == competitionPairing.BlackPlayerIdentifierName);
            string blackName = _blackPlayer == null ? blackParticipant.ParticipantIdentifierName : _blackPlayer.DisplayName;
            string blackRatingFormatted = FormatHelper.FormatRating(blackParticipant.OldRating, clubPreferences.RatingSystem);
            TableNumber = competitionPairing.TableNumber;
            WhitePlayerName = $"{whiteName} ({whiteRatingFormatted})";
            SelectedResult = competitionPairing.Result == null ? string.Empty : DomainEnumStringConverter.Instance.Convert(competitionPairing.Result.Value);
            BlackPlayerName = $"{blackName} ({blackRatingFormatted})";
            GameId = competitionPairing.GameId;
            CanNavigateToGame = canNavigateToGame;
        }

        public int TableNumber { get; set; }

        public string WhitePlayerName { get; set; }

        public string ResultDisplay => SelectedResult;

        public string SelectedResult
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResultDisplay));
                }
            }
        }

        public void CommitSelectedResult()
        {
            GameResult? parsedResult = FormatHelper.ParseGameResultString(SelectedResult);
            OnSetResult?.Invoke(_competitionPairing, parsedResult);
        }

        public string BlackPlayerName { get; set; } = string.Empty;

        public int? GameId { get; }

        public string GameIdDisplay => GameId?.ToString() ?? string.Empty;

        public bool CanNavigateToGame { get; }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
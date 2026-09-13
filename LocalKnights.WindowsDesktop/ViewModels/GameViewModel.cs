using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Helpers;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Displays the details of a specific game, including metadata and PGN text, with game analysis link generation support.
    /// </summary>
    public class GameViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IFileSystemService _fileSystemService;
        private readonly IMessageBoxService _messageBoxService;
        private Game? _currentGame;

        private string _pgn = string.Empty;

        public string Pgn
        {
            get => _pgn;
            set
            {
                _pgn = value;
                OnPropertyChanged(nameof(Pgn));
            }
        }

        public string GameIdValue { get; private set; } = string.Empty;
        public string CreationDateValue { get; private set; } = string.Empty;
        public string ParticipantsValue { get; private set; } = string.Empty;
        public string ResultValue { get; private set; } = string.Empty;
        public string TimeFormatValue { get; private set; } = string.Empty;
        public string RatedValue { get; private set; } = string.Empty;
        public string RatingChangeValue { get; private set; } = string.Empty;
        public string CompetitionIdValue { get; private set; } = string.Empty;

        public string GamePage => $"{AppConstants.DefaultGameIcon} " + LocalizationHelper.GetString("GamePage");
        public string GameIdHelp => LocalizationHelper.GetString("GameIdHelp");
        public string GameCreationDateHelp => LocalizationHelper.GetString("GameCreationDateHelp");
        public string GameParticipantsHelp => LocalizationHelper.GetString("GameParticipantsHelp");
        public string GameDetailTimeFormatHelp => LocalizationHelper.GetString("GameDetailTimeFormatHelp");
        public string GameDetailResultHelp => LocalizationHelper.GetString("GameDetailResultHelp");
        public string GameDetailRatedHelp => LocalizationHelper.GetString("GameDetailRatedHelp");
        public string GameRatingChangeHelp => LocalizationHelper.GetString("GameRatingChangeHelp");
        public string GameCompetitionIdHelp => LocalizationHelper.GetString("GameCompetitionIdHelp");
        public string GameIdTooltip => LocalizationHelper.GetString("GameIdTooltip");
        public string GameCreationDateTooltip => LocalizationHelper.GetString("GameCreationDateTooltip");
        public string GameParticipantsTooltip => LocalizationHelper.GetString("GameParticipantsTooltip");
        public string GameDetailTimeFormatTooltip => LocalizationHelper.GetString("GameDetailTimeFormatTooltip");
        public string GameDetailResultTooltip => LocalizationHelper.GetString("GameDetailResultTooltip");
        public string GameDetailRatedTooltip => LocalizationHelper.GetString("GameDetailRatedTooltip");
        public string GameRatingChangeTooltip => LocalizationHelper.GetString("GameRatingChangeTooltip");
        public string GameCompetitionIdTooltip => LocalizationHelper.GetString("GameCompetitionIdTooltip");
        public string GameDetailPgnHelp => LocalizationHelper.GetString("GameDetailPgnHelp");
        public string GameDetailPgnTooltip => LocalizationHelper.GetString("GameDetailPgnTooltip");
        public string GameIdLabel => LocalizationHelper.GetString("GameIdLabel");
        public string CreationDateLabel => LocalizationHelper.GetString("CreationDateLabel");
        public string ParticipantsLabel => LocalizationHelper.GetString("ParticipantsLabel");
        public string ResultLabel => LocalizationHelper.GetString("ResultLabel");
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");
        public string RatingChangeLabel => LocalizationHelper.GetString("RatingChangeLabel");
        public string CompetitionIdLabel => LocalizationHelper.GetString("CompetitionIdLabel");
        public string PgnLabel => LocalizationHelper.GetString("PgnLabel");
        public string SavePgnButtonLabel => LocalizationHelper.GetString("SavePgnButtonLabel");
        public string AnalyzeOnLichessButtonLabel => "🔬 " + LocalizationHelper.GetString("AnalyzeOnLichessButtonLabel");
        public string DeleteGameButton => LocalizationHelper.GetString("DeleteGameButton");
        public string BackToGamesButtonLabel => LocalizationHelper.GetString("BackButtonLabel");

        public ICommand SavePgnCommand { get; }
        public ICommand AnalyzeOnLichessCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public GameViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IGameService gameService,
            IPlayerService playerService,
            IFileSystemService fileSystemService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _gameService = gameService;
            _playerService = playerService;
            _fileSystemService = fileSystemService;
            _messageBoxService = messageBoxService;

            SavePgnCommand = new RelayCommand(async _ => await SavePgnAsync());
            AnalyzeOnLichessCommand = new RelayCommand(_ => AnalyzeOnLichess());
            DeleteCommand = new RelayCommand(async _ => await DeleteGameAsync());
            BackCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());
        }

        public override async void PageTransitionSetup(IDictionary<string, object> parameters)
        {
            if (parameters.TryGetValue(nameof(Game.Id), out object? idObj) && idObj is int gameId)
            {
                await LoadGameAsync(gameId);
            }
            else
            {
                _logger.LogWarning($"No GameId parameter provided for {nameof(GameViewModel)}");
            }
        }

        private async Task LoadGameAsync(int gameId)
        {
            try
            {
                if (gameId <= 0)
                {
                    _messageBoxService.ShowError(LocalizationHelper.GetString("InvalidGameSelectionMessage"), LocalizationHelper.GetString("Error"));
                    BackCommand.Execute(null);
                    return;
                }

                _currentGame = await _gameService.GetGameByIdAsync(gameId);
                if (_currentGame == null)
                {
                    _messageBoxService.ShowError(LocalizationHelper.GetString("GameNotFoundMessage"), LocalizationHelper.GetString("Error"));
                    BackCommand.Execute(null);
                    return;
                }

                var yesLabel = LocalizationHelper.GetString("YesLabel");
                var noLabel = LocalizationHelper.GetString("NoLabel");
                var notAvailable = LocalizationHelper.GetString("NotAvailableShort");

                Player? whitePlayer = _playerService.GetPlayerByIdentifierAsync(_currentGame.WhitePlayerId).GetAwaiter().GetResult();
                Player? blackPlayer = _playerService.GetPlayerByIdentifierAsync(_currentGame.BlackPlayerId).GetAwaiter().GetResult();

                GameIdValue = _currentGame.Id.ToString(CultureInfo.CurrentCulture);
                CreationDateValue = _currentGame.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
                ParticipantsValue = FormatHelper.FormatGameParticipants(_currentGame, whitePlayer, blackPlayer);
                ResultValue = DomainEnumStringConverter.Instance.Convert(_currentGame.Result);
                TimeFormatValue = DomainEnumStringConverter.Instance.Convert(_currentGame.TimeFormat);
                RatedValue = _currentGame.IsRated ? yesLabel : noLabel;
                RatingChangeValue = FormatHelper.FormatRatingChange(_currentGame.WhitePlayerRatingChange, _currentGame.BlackPlayerRatingChange);
                int? compId = await _gameService.GetCompetitionIdForGameAsync(_currentGame.Id);
                CompetitionIdValue = compId?.ToString(CultureInfo.CurrentCulture) ?? notAvailable;
                Pgn = _currentGame.Pgn ?? string.Empty;

                OnPropertyChanged(nameof(GameIdValue));
                OnPropertyChanged(nameof(CreationDateValue));
                OnPropertyChanged(nameof(ParticipantsValue));
                OnPropertyChanged(nameof(ResultValue));
                OnPropertyChanged(nameof(TimeFormatValue));
                OnPropertyChanged(nameof(RatedValue));
                OnPropertyChanged(nameof(RatingChangeValue));
                OnPropertyChanged(nameof(CompetitionIdValue));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading game: {ex.Message}", LocalizationHelper.GetString("Error"));
                BackCommand.Execute(null);
            }
        }

        private async Task SavePgnAsync()
        {
            if (_currentGame == null)
            {
                return;
            }

            try
            {
                bool updated = await Task.Run(async () => await _gameService.UpdateGamePgnAsync(_currentGame.Id, Pgn));

                if (!updated)
                {
                    _messageBoxService.ShowError(LocalizationHelper.GetString("GameNotFoundMessage"), LocalizationHelper.GetString("Error"));
                    return;
                }

                _messageBoxService.ShowInfo(LocalizationHelper.GetString("GameSavedMessage"), LocalizationHelper.GetString("Success"));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error saving game: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private async Task DeleteGameAsync()
        {
            if (_currentGame == null)
            {
                return;
            }

            CustomMessageBoxResult result = _messageBoxService.ShowConfirmation(
                LocalizationHelper.GetString("DeleteGameConfirmation"),
                LocalizationHelper.GetString("Confirm"));

            if (result != CustomMessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _gameService.DeleteGameAsync(_currentGame.Id);
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("GameDeletedMessage"), LocalizationHelper.GetString("Success"));
                BackCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error deleting game: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private void AnalyzeOnLichess()
        {
            if (!PgnAnalysisHelper.TryBuildLichessAnalysisUrl(Pgn, out var analysisUrl))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("InvalidPgnForAnalysisMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            try
            {
                _fileSystemService.OpenUrl(analysisUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open Lichess analysis URL: {analysisUrl}. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                _messageBoxService.ShowError(LocalizationHelper.GetString("OpenLichessFailedMessage"), LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(GamePage));
            OnPropertyChanged(nameof(GameIdHelp));
            OnPropertyChanged(nameof(GameCreationDateHelp));
            OnPropertyChanged(nameof(GameParticipantsHelp));
            OnPropertyChanged(nameof(GameDetailTimeFormatHelp));
            OnPropertyChanged(nameof(GameDetailResultHelp));
            OnPropertyChanged(nameof(GameDetailRatedHelp));
            OnPropertyChanged(nameof(GameRatingChangeHelp));
            OnPropertyChanged(nameof(GameCompetitionIdHelp));
            OnPropertyChanged(nameof(GameDetailPgnHelp));
            OnPropertyChanged(nameof(GameIdTooltip));
            OnPropertyChanged(nameof(GameCreationDateTooltip));
            OnPropertyChanged(nameof(GameParticipantsTooltip));
            OnPropertyChanged(nameof(GameDetailTimeFormatTooltip));
            OnPropertyChanged(nameof(GameDetailResultTooltip));
            OnPropertyChanged(nameof(GameDetailRatedTooltip));
            OnPropertyChanged(nameof(GameRatingChangeTooltip));
            OnPropertyChanged(nameof(GameCompetitionIdTooltip));
            OnPropertyChanged(nameof(GameDetailPgnTooltip));
            OnPropertyChanged(nameof(GameIdLabel));
            OnPropertyChanged(nameof(CreationDateLabel));
            OnPropertyChanged(nameof(ParticipantsLabel));
            OnPropertyChanged(nameof(ResultLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(RatingChangeLabel));
            OnPropertyChanged(nameof(CompetitionIdLabel));
            OnPropertyChanged(nameof(PgnLabel));
            OnPropertyChanged(nameof(SavePgnButtonLabel));
            OnPropertyChanged(nameof(AnalyzeOnLichessButtonLabel));
            OnPropertyChanged(nameof(DeleteGameButton));
            OnPropertyChanged(nameof(BackToGamesButtonLabel));
            LoadGameAsync(_currentGame.Id).ConfigureAwait(false);
        }
    }
}

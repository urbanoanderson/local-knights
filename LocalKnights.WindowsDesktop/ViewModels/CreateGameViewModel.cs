using System.Collections.ObjectModel;
using System.Windows.Input;
using LocalKnights.Common.Models;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Handles the creation of new games with validation for player selection (exactly two different players required) and game data formatting.
    /// </summary>
    public class CreateGameViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IMessageBoxService _messageBoxService;

        private string _whitePlayerQuery = string.Empty;
        private string _blackPlayerQuery = string.Empty;
        private bool _isWhiteSearchFocused;
        private bool _isBlackSearchFocused;
        private Player? _selectedWhitePlayer;
        private Player? _selectedBlackPlayer;
        private string _selectedWhitePlayerDisplay = string.Empty;
        private string _selectedBlackPlayerDisplay = string.Empty;
        private TimeFormat _selectedTimeFormat = CommonConstants.DefaultTimeFormat;
        private GameResult _selectedResult = GameResult.Draw;
        private bool _isRated;
        private string _pgn = string.Empty;

        public ObservableCollection<string> WhiteSuggestions { get; } = new();
        public ObservableCollection<string> BlackSuggestions { get; } = new();
        public ObservableCollection<TimeFormat> TimeFormats { get; } = new();
        public ObservableCollection<GameResult> Results { get; } = new();

        public string WhitePlayerQuery
        {
            get => _whitePlayerQuery;
            set
            {
                _whitePlayerQuery = value;
                OnPropertyChanged(nameof(WhitePlayerQuery));
                RefreshWhiteSuggestions();
            }
        }

        public string BlackPlayerQuery
        {
            get => _blackPlayerQuery;
            set
            {
                _blackPlayerQuery = value;
                OnPropertyChanged(nameof(BlackPlayerQuery));
                RefreshBlackSuggestions();
            }
        }

        public bool IsWhiteSearchFocused
        {
            get => _isWhiteSearchFocused;
            private set
            {
                if (_isWhiteSearchFocused != value)
                {
                    _isWhiteSearchFocused = value;
                    OnPropertyChanged(nameof(IsWhiteSearchFocused));
                    OnPropertyChanged(nameof(ShowWhiteSuggestions));
                }
            }
        }

        public bool IsBlackSearchFocused
        {
            get => _isBlackSearchFocused;
            private set
            {
                if (_isBlackSearchFocused != value)
                {
                    _isBlackSearchFocused = value;
                    OnPropertyChanged(nameof(IsBlackSearchFocused));
                    OnPropertyChanged(nameof(ShowBlackSuggestions));
                }
            }
        }

        public Player? SelectedWhitePlayer
        {
            get => _selectedWhitePlayer;
            set
            {
                _selectedWhitePlayer = value;
                _selectedWhitePlayerDisplay = value == null ? string.Empty : $"{value.DisplayName} ({value.IdentifierName})";
                OnPropertyChanged(nameof(SelectedWhitePlayer));
                OnPropertyChanged(nameof(IsWhitePlayerSelected));
                OnPropertyChanged(nameof(SelectedWhitePlayerDisplay));
                RefreshBlackSuggestions();
            }
        }

        public Player? SelectedBlackPlayer
        {
            get => _selectedBlackPlayer;
            set
            {
                _selectedBlackPlayer = value;
                _selectedBlackPlayerDisplay = value == null ? string.Empty : $"{value.DisplayName} ({value.IdentifierName})";
                OnPropertyChanged(nameof(SelectedBlackPlayer));
                OnPropertyChanged(nameof(IsBlackPlayerSelected));
                OnPropertyChanged(nameof(SelectedBlackPlayerDisplay));
                RefreshWhiteSuggestions();
            }
        }

        public TimeFormat SelectedTimeFormat
        {
            get => _selectedTimeFormat;
            set
            {
                if (_selectedTimeFormat != value)
                {
                    _selectedTimeFormat = value;
                    OnPropertyChanged(nameof(SelectedTimeFormat));
                }
            }
        }

        public GameResult SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult != value)
                {
                    _selectedResult = value;
                    OnPropertyChanged(nameof(SelectedResult));
                }
            }
        }

        public bool IsRated
        {
            get => _isRated;
            set
            {
                _isRated = value;
                OnPropertyChanged(nameof(IsRated));
            }
        }

        public string Pgn
        {
            get => _pgn;
            set
            {
                _pgn = value;
                OnPropertyChanged(nameof(Pgn));
            }
        }

        public string CreateGame => $"{AppConstants.DefaultGameIcon} " + LocalizationHelper.GetString("CreateGame");
        public string CreateGameWhitePlayerHelp => LocalizationHelper.GetString("CreateGameWhitePlayerHelp");
        public string CreateGameBlackPlayerHelp => LocalizationHelper.GetString("CreateGameBlackPlayerHelp");
        public string CreateGameTimeFormatHelp => LocalizationHelper.GetString("CreateGameTimeFormatHelp");
        public string CreateGameResultHelp => LocalizationHelper.GetString("CreateGameResultHelp");
        public string CreateGameRatedHelp => LocalizationHelper.GetString("CreateGameRatedHelp");
        public string CreateGamePgnHelp => LocalizationHelper.GetString("CreateGamePgnHelp");
        public string CreateGameWhitePlayerTooltip => LocalizationHelper.GetString("CreateGameWhitePlayerTooltip");
        public string CreateGameBlackPlayerTooltip => LocalizationHelper.GetString("CreateGameBlackPlayerTooltip");
        public string CreateGameTimeFormatTooltip => LocalizationHelper.GetString("CreateGameTimeFormatTooltip");
        public string CreateGameResultTooltip => LocalizationHelper.GetString("CreateGameResultTooltip");
        public string CreateGameRatedTooltip => LocalizationHelper.GetString("CreateGameRatedTooltip");
        public string CreateGamePgnTooltip => LocalizationHelper.GetString("CreateGamePgnTooltip");
        public string ParticipantsLabel => LocalizationHelper.GetString("ParticipantsLabel");
        public string WhitePlayerLabel => LocalizationHelper.GetString("WhitePlayerLabel");
        public string BlackPlayerLabel => LocalizationHelper.GetString("BlackPlayerLabel");
        public string SearchPlaceholder => $"{AppConstants.DefaultSearchIcon} {LocalizationHelper.GetString("PlayersSearchPlaceholder")}";
        public string SelectedPlayerLabel => LocalizationHelper.GetString("SelectedPlayerLabel");
        public string RemoveParticipantButtonLabel => LocalizationHelper.GetString("RemoveParticipantButtonLabel");
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");
        public string ResultLabel => LocalizationHelper.GetString("ResultLabel");
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");
        public string PgnLabel => LocalizationHelper.GetString("PgnLabel");
        public string CreateButtonLabel => LocalizationHelper.GetString("CreateButtonLabel");
        public string CancelLabel => LocalizationHelper.GetString("CancelLabel");

        public bool HasWhiteSuggestions => WhiteSuggestions.Count > 0;
        public bool HasBlackSuggestions => BlackSuggestions.Count > 0;
        public bool ShowWhiteSuggestions => IsWhiteSearchFocused && HasWhiteSuggestions && !string.IsNullOrWhiteSpace(WhitePlayerQuery);
        public bool ShowBlackSuggestions => IsBlackSearchFocused && HasBlackSuggestions && !string.IsNullOrWhiteSpace(BlackPlayerQuery);
        public bool IsWhitePlayerSelected => SelectedWhitePlayer != null;
        public bool IsBlackPlayerSelected => SelectedBlackPlayer != null;
        public string SelectedWhitePlayerDisplay => _selectedWhitePlayerDisplay;
        public string SelectedBlackPlayerDisplay => _selectedBlackPlayerDisplay;

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectWhiteSuggestionCommand { get; }
        public ICommand SelectBlackSuggestionCommand { get; }
        public ICommand RemoveWhitePlayerCommand { get; }
        public ICommand RemoveBlackPlayerCommand { get; }

        public CreateGameViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IGameService gameService,
            IPlayerService playerService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _gameService = gameService;
            _playerService = playerService;
            _messageBoxService = messageBoxService;

            CreateCommand = new RelayCommand(async _ => await CreateGameAsync());
            CancelCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());
            SelectWhiteSuggestionCommand = new RelayCommand(SelectWhiteSuggestion);
            SelectBlackSuggestionCommand = new RelayCommand(SelectBlackSuggestion);
            RemoveWhitePlayerCommand = new RelayCommand(_ => RemoveWhitePlayer());
            RemoveBlackPlayerCommand = new RelayCommand(_ => RemoveBlackPlayer());

            LoadInitialDataAsync().ConfigureAwait(false);
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                TimeFormats.Clear();
                foreach (TimeFormat timeFormat in Enum.GetValues(typeof(TimeFormat)).Cast<TimeFormat>())
                {
                    TimeFormats.Add(timeFormat);
                }

                Results.Clear();
                foreach (GameResult gameResult in Enum.GetValues(typeof(GameResult)).Cast<GameResult>())
                {
                    Results.Add(gameResult);
                }

                if (!TimeFormats.Contains(SelectedTimeFormat))
                {
                    SelectedTimeFormat = CommonConstants.DefaultTimeFormat;
                }
                else
                {
                    OnPropertyChanged(nameof(SelectedTimeFormat));
                }
                if (!Results.Contains(SelectedResult))
                {
                    SelectedResult = GameResult.Draw;
                }
                else
                {
                    OnPropertyChanged(nameof(SelectedResult));
                }

                RefreshWhiteSuggestions();
                RefreshBlackSuggestions();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading game form: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private void SelectWhiteSuggestion(object? parameter)
        {
            if (parameter is not string selectedSuggestion)
            {
                return;
            }

            Player? selected = ResolveSuggestion(selectedSuggestion, SelectedBlackPlayer?.IdentifierName);
            if (selected == null)
            {
                return;
            }

            if (SelectedBlackPlayer != null && SelectedBlackPlayer.IdentifierName == selected.IdentifierName)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("SelectDifferentPlayersMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            SelectedWhitePlayer = selected;
            WhitePlayerQuery = string.Empty;
            WhiteSuggestions.Clear();
            OnPropertyChanged(nameof(HasWhiteSuggestions));
        }

        private void SelectBlackSuggestion(object? parameter)
        {
            if (parameter is not string selectedSuggestion)
            {
                return;
            }

            Player? selected = ResolveSuggestion(selectedSuggestion, SelectedWhitePlayer?.IdentifierName);
            if (selected == null)
            {
                return;
            }

            if (SelectedWhitePlayer != null && SelectedWhitePlayer.IdentifierName == selected.IdentifierName)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("SelectDifferentPlayersMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            SelectedBlackPlayer = selected;
            BlackPlayerQuery = string.Empty;
            BlackSuggestions.Clear();
            OnPropertyChanged(nameof(HasBlackSuggestions));
        }

        private void RemoveWhitePlayer()
        {
            SelectedWhitePlayer = null;
            RefreshWhiteSuggestions();
            RefreshBlackSuggestions();
        }

        private void RemoveBlackPlayer()
        {
            SelectedBlackPlayer = null;
            RefreshWhiteSuggestions();
            RefreshBlackSuggestions();
        }

        public void SetWhiteSearchFocus(bool isFocused)
        {
            IsWhiteSearchFocused = isFocused;
        }

        public void SetBlackSearchFocus(bool isFocused)
        {
            IsBlackSearchFocused = isFocused;
        }

        private void RefreshWhiteSuggestions()
        {
            UpdateSuggestionsAsync(WhiteSuggestions, WhitePlayerQuery, SelectedBlackPlayer?.IdentifierName);
            OnPropertyChanged(nameof(HasWhiteSuggestions));
            OnPropertyChanged(nameof(ShowWhiteSuggestions));
        }

        private void RefreshBlackSuggestions()
        {
            UpdateSuggestionsAsync(BlackSuggestions, BlackPlayerQuery, SelectedWhitePlayer?.IdentifierName);
            OnPropertyChanged(nameof(HasBlackSuggestions));
            OnPropertyChanged(nameof(ShowBlackSuggestions));
        }

        private async void UpdateSuggestionsAsync(ObservableCollection<string> target, string query, string? excludedPlayerIdentifier)
        {
            target.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            List<Player> ranked = await _playerService.SearchPlayersAsync(query, excludedPlayerIdentifier, 8);

            if (target == null)
            {
                return;
            }

            foreach (Player player in ranked)
            {
                target.Add($"{player.DisplayName} ({player.IdentifierName})");
            }
        }

        private Player? ResolveSuggestion(string selectedSuggestion, string? excludedPlayerIdentifier)
        {
            string identifierName = ExtractIdentifierName(selectedSuggestion);
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                return null;
            }

            List<Player> matches = _playerService.SearchPlayersAsync(identifierName, excludedPlayerIdentifier, 1).GetAwaiter().GetResult();
            return matches.FirstOrDefault(player => player.IdentifierName == identifierName);
        }

        private static string ExtractIdentifierName(string suggestion)
        {
            int openingParenthesisIndex = suggestion.LastIndexOf('(');
            int closingParenthesisIndex = suggestion.LastIndexOf(')');

            if (openingParenthesisIndex < 0 || closingParenthesisIndex <= openingParenthesisIndex)
            {
                return string.Empty;
            }

            return suggestion.Substring(openingParenthesisIndex + 1, closingParenthesisIndex - openingParenthesisIndex - 1).Trim();
        }

        private async Task CreateGameAsync()
        {
            if (SelectedWhitePlayer == null || SelectedBlackPlayer == null)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("SelectTwoPlayersMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (SelectedWhitePlayer.IdentifierName == SelectedBlackPlayer.IdentifierName)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("SelectDifferentPlayersMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            try
            {
                await _gameService.CreateGameAsync(
                    SelectedWhitePlayer.IdentifierName,
                    SelectedBlackPlayer.IdentifierName,
                    SelectedTimeFormat,
                    SelectedResult,
                    IsRated,
                    Pgn);

                _messageBoxService.ShowInfo(LocalizationHelper.GetString("GameCreatedMessage"), LocalizationHelper.GetString("Success"));
                _pageNavigationService.HardNavigateTo<GamesViewModel>();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error creating game: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(CreateGame));
            OnPropertyChanged(nameof(CreateGameWhitePlayerHelp));
            OnPropertyChanged(nameof(CreateGameBlackPlayerHelp));
            OnPropertyChanged(nameof(CreateGameTimeFormatHelp));
            OnPropertyChanged(nameof(CreateGameResultHelp));
            OnPropertyChanged(nameof(CreateGameRatedHelp));
            OnPropertyChanged(nameof(CreateGamePgnHelp));
            OnPropertyChanged(nameof(CreateGameWhitePlayerTooltip));
            OnPropertyChanged(nameof(CreateGameBlackPlayerTooltip));
            OnPropertyChanged(nameof(CreateGameTimeFormatTooltip));
            OnPropertyChanged(nameof(CreateGameResultTooltip));
            OnPropertyChanged(nameof(CreateGameRatedTooltip));
            OnPropertyChanged(nameof(CreateGamePgnTooltip));
            OnPropertyChanged(nameof(ParticipantsLabel));
            OnPropertyChanged(nameof(WhitePlayerLabel));
            OnPropertyChanged(nameof(BlackPlayerLabel));
            OnPropertyChanged(nameof(SearchPlaceholder));
            OnPropertyChanged(nameof(SelectedPlayerLabel));
            OnPropertyChanged(nameof(RemoveParticipantButtonLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(ResultLabel));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(PgnLabel));
            OnPropertyChanged(nameof(CreateButtonLabel));
            OnPropertyChanged(nameof(CancelLabel));
            LoadInitialDataAsync().ConfigureAwait(false);
        }
    }
}

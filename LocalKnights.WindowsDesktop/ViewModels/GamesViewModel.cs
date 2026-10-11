using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    public class GamesViewModel : BaseAppViewModel
    {
        private const string PageNumberParameterKey = "GamesPageNumber";
        private const string PageSizeParameterKey = "GamesPageSize";
        private const string SortParameterKey = "GamesSortParameter";

        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUISettingsService _uiSettingsService;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IMessageBoxService _messageBoxService;
        private ObservableCollection<GamesPageTableItem> _games = new();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalGamesCount = 0;
        private bool _isPaginationInProgress;
        private bool _refreshPending;
        private SortParameter _sortParameter = new(nameof(Game.CreatedAt), ListSortDirection.Descending);

        /// <summary>
        /// Exposes the current sort parameter for UI consumers (e.g., to update sort indicators).
        /// </summary>
        public SortParameter CurrentSortParameter => _sortParameter;

        public ObservableCollection<GamesPageTableItem> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged(nameof(Games));
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                var clamped = Math.Clamp(value, 1, Math.Max(1, TotalPages));
                if (_currentPage != clamped)
                {
                    _currentPage = clamped;
                    OnPropertyChanged(nameof(CurrentPage));
                    RefreshPagedGamesAsync().ConfigureAwait(false);
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            private set
            {
                var normalized = Math.Max(1, value);
                if (_totalPages != normalized)
                {
                    _totalPages = normalized;
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        public bool IsPaginationInProgress
        {
            get => _isPaginationInProgress;
            private set
            {
                if (_isPaginationInProgress != value)
                {
                    _isPaginationInProgress = value;
                    OnPropertyChanged(nameof(IsPaginationInProgress));
                    OnPropertyChanged(nameof(CanGoToPreviousPage));
                    OnPropertyChanged(nameof(CanGoToNextPage));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanGoToPreviousPage => TotalPages > 1 && !IsPaginationInProgress;
        public bool CanGoToNextPage => TotalPages > 1 && !IsPaginationInProgress;
        public string CurrentPageDisplay => string.Format(LocalizationHelper.GetString("PageIndicator"), CurrentPage, TotalPages);

        public string GamesPage => $"{AppConstants.DefaultGameIcon} " + LocalizationHelper.GetString("GamesPage");
        public string GameIdLabel => LocalizationHelper.GetString("GameIdLabel");
        public string CreationDateLabel => LocalizationHelper.GetString("CreationDateLabel");
        public string ParticipantsLabel => LocalizationHelper.GetString("ParticipantsLabel");
        public string ResultLabel => LocalizationHelper.GetString("ResultLabel");
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");
        public string RatingChangeLabel => LocalizationHelper.GetString("RatingChangeLabel");
        public string CompetitionIdLabel => LocalizationHelper.GetString("CompetitionIdLabel");
        public string CreateGameButtonLabel => LocalizationHelper.GetString("CreateGameButtonLabel");
        public string BackButtonLabel => LocalizationHelper.GetString("BackButtonLabel");
        public string PreviousPageButtonLabel => LocalizationHelper.GetString("PreviousPageButtonLabel");
        public string NextPageButtonLabel => LocalizationHelper.GetString("NextPageButtonLabel");

        public ICommand CreateGameCommand { get; }
        public ICommand SelectGameCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand ChangeSortCommand { get; }

        public GamesViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IUISettingsService uiSettingsService,
            IGameService gameService,
            IPlayerService playerService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _uiSettingsService = uiSettingsService;
            _gameService = gameService;
            _playerService = playerService;
            _messageBoxService = messageBoxService;

            CreateGameCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<CreateGameViewModel>());
            SelectGameCommand = new RelayCommand(SelectGame);
            BackCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CanGoToPreviousPage);
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CanGoToNextPage);
            ChangeSortCommand = new RelayCommand(ChangeSortColumn);
        }

        public override async void PageTransitionSetup(IDictionary<string, object> parameters)
        {
            int? savedPageSize = parameters.TryGetValue(PageSizeParameterKey, out object? savedPageSizeValue) && savedPageSizeValue is int pageSize
                ? pageSize
                : null;
            bool canRestoreTableState = savedPageSize.HasValue && savedPageSize.Value == _uiSettingsService.CurrentAppSettings.PageSize;

            if (canRestoreTableState)
            {
                if (parameters.TryGetValue(SortParameterKey, out object? savedSortParameterValue) && savedSortParameterValue is SortParameter savedSortParameter)
                {
                    _sortParameter = new SortParameter(savedSortParameter.SortColumn, savedSortParameter.SortDirection);
                    OnPropertyChanged(nameof(CurrentSortParameter));
                }

                if (parameters.TryGetValue(PageNumberParameterKey, out object? savedPageNumberValue) && savedPageNumberValue is int savedPageNumber)
                {
                    _currentPage = Math.Max(1, savedPageNumber);
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
            else
            {
                _currentPage = 1;
                OnPropertyChanged(nameof(CurrentPage));
            }

            await LoadGamesAsync().ConfigureAwait(false);
        }

        private async Task LoadGamesAsync()
        {
            try
            {
                int pageSize = Math.Max(1, _uiSettingsService.CurrentAppSettings.PageSize);
                _totalGamesCount = await _gameService.GetGamesCountAsync();
                TotalPages = (int)Math.Ceiling((double)_totalGamesCount / pageSize);

                if (CurrentPage > TotalPages)
                {
                    _currentPage = Math.Max(1, TotalPages);
                    OnPropertyChanged(nameof(CurrentPage));
                }

                await RefreshPagedGamesAsync();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading games: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private async Task RefreshPagedGamesAsync()
        {
            if (IsPaginationInProgress)
            {
                _refreshPending = true;
                return;
            }

            try
            {
                _refreshPending = false;
                IsPaginationInProgress = true;
                int pageSize = Math.Max(1, _uiSettingsService.CurrentAppSettings.PageSize);
                TotalPages = (int)Math.Ceiling((double)_totalGamesCount / pageSize);

                if (_currentPage > TotalPages)
                {
                    _currentPage = Math.Max(1, TotalPages);
                    OnPropertyChanged(nameof(CurrentPage));
                }

                if (_currentPage < 1)
                {
                    _currentPage = 1;
                    OnPropertyChanged(nameof(CurrentPage));
                }

                string yesLabel = LocalizationHelper.GetString("YesLabel");
                string noLabel = LocalizationHelper.GetString("NoLabel");
                string notAvailable = LocalizationHelper.GetString("NotAvailableShort");

                // Fetch only the games for the current page from the database with current sort
                List<Game> pagedGames = await Task.Run(async () => await _gameService.GetGamesPagedAsync(_currentPage, pageSize, _sortParameter));

                var displayGames = new List<GamesPageTableItem>();
                foreach (Game g in pagedGames)
                {
                    Player? whitePlayer = await _playerService.GetPlayerByIdentifierAsync(g.WhitePlayerId);
                    Player? blackPlayer = await _playerService.GetPlayerByIdentifierAsync(g.BlackPlayerId);

                    int? compId = await _gameService.GetCompetitionIdForGameAsync(g.Id);
                    displayGames.Add(new GamesPageTableItem
                    {
                        Id = g.Id,
                        CreatedAtSortKey = g.CreatedAt,
                        CreatedAt = g.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture),
                        Participants = FormatHelper.FormatGameParticipants(g, whitePlayer, blackPlayer),
                        Result = DomainEnumStringConverter.Instance.Convert(g.Result),
                        TimeFormat = DomainEnumStringConverter.Instance.Convert(g.TimeFormat),
                        Rated = g.IsRated ? yesLabel : noLabel,
                        RatingChange = FormatHelper.FormatRatingChange(g.WhitePlayerRatingChange, g.BlackPlayerRatingChange),
                        CompetitionId = compId?.ToString() ?? string.Empty
                    });
                }

                Games.Clear();
                foreach (GamesPageTableItem displayGame in displayGames)
                {
                    Games.Add(displayGame);
                }

                OnPropertyChanged(nameof(Games));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(CurrentPageDisplay));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading games: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
            finally
            {
                IsPaginationInProgress = false;
                if (_refreshPending)
                {
                    _ = RefreshPagedGamesAsync();
                }
            }
        }

        private void ChangeSortColumn(object? parameter)
        {
            string columnName = parameter as string ?? nameof(Game.CreatedAt);

            // If clicking the same column, reverse the sort direction
            if (_sortParameter.SortColumn == columnName)
            {
                ListSortDirection nextDirection = _sortParameter.SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
                _sortParameter = new SortParameter(columnName, nextDirection);
            }
            else
            {
                // New column - sort ascending by default (descending for CreatedAt for newest first)
                ListSortDirection defaultDirection = columnName == nameof(Game.CreatedAt)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
                _sortParameter = new SortParameter(columnName, defaultDirection);
            }

            // Reset to first page and refresh with new sort
            _currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentSortParameter));
            RefreshPagedGamesAsync().ConfigureAwait(false);
        }

        private void SelectGame(object? parameter)
        {
            if (parameter is GamesPageTableItem game)
            {
                _pageNavigationService.NavigateTo<GameViewModel>(
                    pageParameters: new Dictionary<string, object>
                    {
                        { nameof(Game.Id), game.Id }
                    },
                    backParameters: new Dictionary<string, object>
                    {
                        { PageNumberParameterKey, CurrentPage },
                        { PageSizeParameterKey, _uiSettingsService.CurrentAppSettings.PageSize },
                        { SortParameterKey, new SortParameter(_sortParameter.SortColumn, _sortParameter.SortDirection) }
                    });
            }
        }

        private void GoToPreviousPage()
        {
            if (TotalPages <= 1)
            {
                return;
            }

            if (CurrentPage <= 1)
            {
                CurrentPage = TotalPages;
            }
            else
            {
                CurrentPage--;
            }
        }

        private void GoToNextPage()
        {
            if (TotalPages <= 1)
            {
                return;
            }

            if (CurrentPage >= TotalPages)
            {
                CurrentPage = 1;
            }
            else
            {
                CurrentPage++;
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            OnPropertyChanged(nameof(Games));
            OnPropertyChanged(nameof(GamesPage));
            OnPropertyChanged(nameof(GameIdLabel));
            OnPropertyChanged(nameof(CreationDateLabel));
            OnPropertyChanged(nameof(ParticipantsLabel));
            OnPropertyChanged(nameof(ResultLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(RatingChangeLabel));
            OnPropertyChanged(nameof(CompetitionIdLabel));
            OnPropertyChanged(nameof(CreateGameButtonLabel));
            OnPropertyChanged(nameof(BackButtonLabel));
            OnPropertyChanged(nameof(PreviousPageButtonLabel));
            OnPropertyChanged(nameof(NextPageButtonLabel));
            OnPropertyChanged(nameof(CurrentPageDisplay));
            RefreshPagedGamesAsync().ConfigureAwait(false);
        }

        public override void OnPageSizeChanged(int _)
        {
            CurrentPage = 1;
            RefreshPagedGamesAsync().ConfigureAwait(false);
        }
    }
}

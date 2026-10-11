using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Players view that manages the display and pagination of club players.
    /// Provides functionality to create, edit, delete, and refresh player information from the database.
    /// </summary>
    public class PlayersViewModel : BaseAppViewModel
    {
        private const string PageNumberParameterKey = "PlayersPageNumber";
        private const string PageSizeParameterKey = "PlayersPageSize";
        private const string SearchQueryParameterKey = "PlayersSearchQuery";
        private const string SortParameterKey = "PlayersSortParameter";

        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUISettingsService _uiSettingsService;
        private readonly IClubService _clubService;
        private readonly IPlayerService _playerService;
        private readonly IMessageBoxService _messageBoxService;

        private ObservableCollection<PlayersPageTableItem> _players = new();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalPlayersCount = 0;
        private int _filteredPlayersCount = 0;
        private bool _isPaginationInProgress;
        private bool _refreshPending;
        private string _searchQuery = string.Empty;
        private SortParameter _sortParameter = new(nameof(Player.IdentifierName), ListSortDirection.Ascending);

        /// <summary>
        /// Exposes the current sort parameter for UI consumers (e.g., to update sort indicators).
        /// </summary>
        public SortParameter CurrentSortParameter => _sortParameter;

        /// <summary>
        /// Gets or sets the collection of players to display on the current page.
        /// </summary>
        public ObservableCollection<PlayersPageTableItem> Players
        {
            get => _players;
            set
            {
                _players = value;
                OnPropertyChanged(nameof(Players));
            }
        }

        /// <summary>
        /// Gets or sets the current page number for pagination.
        /// Values are clamped between 1 and the total number of pages.
        /// </summary>
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
                    RefreshPagedPlayersAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets the total number of pages available for pagination.
        /// This value is automatically calculated based on the total player count and the page size.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether a pagination operation is currently in progress.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the user can navigate to the previous page.
        /// The button stays available when there is more than one page, enabling wrap-around navigation.
        /// </summary>
        public bool CanGoToPreviousPage => TotalPages > 1 && !IsPaginationInProgress;

        /// <summary>
        /// Gets a value indicating whether the user can navigate to the next page.
        /// The button stays available when there is more than one page, enabling wrap-around navigation.
        /// </summary>
        public bool CanGoToNextPage => TotalPages > 1 && !IsPaginationInProgress;

        /// <summary>
        /// Gets the formatted page indicator string showing the current page and total pages.
        /// </summary>
        public string CurrentPageDisplay => string.Format(LocalizationHelper.GetString("PageIndicator"), CurrentPage, TotalPages);

        /// <summary>
        /// Gets the localized label for the Players page.
        /// </summary>
        public string PlayersPage => $"{AppConstants.DefaultPlayerIcon} " + LocalizationHelper.GetString("PlayersPage");
        public string AgeLabel => LocalizationHelper.GetString("AgeLabel");
        public string IdentifierNameLabel => LocalizationHelper.GetString("IdentifierNameLabel");
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");
        public string ChessTitleLabel => LocalizationHelper.GetString("ChessTitleLabel");
        public string CreationDateLabel => LocalizationHelper.GetString("CreationDateLabel");
        public string BulletRatingLabel => LocalizationHelper.GetString("TimeFormat_Bullet");
        public string BlitzRatingLabel => LocalizationHelper.GetString("TimeFormat_Blitz");
        public string RapidRatingLabel => LocalizationHelper.GetString("TimeFormat_Rapid");
        public string ClassicalRatingLabel => LocalizationHelper.GetString("TimeFormat_Classical");
        public string CreatePlayerButtonLabel => LocalizationHelper.GetString("CreatePlayerButtonLabel");
        public string BulkImportButtonLabel => LocalizationHelper.GetString("BulkImportButtonLabel");
        public string BulkExportButtonLabel => LocalizationHelper.GetString("BulkExportButtonLabel");
        public string BackButtonLabel => LocalizationHelper.GetString("BackButtonLabel");
        public string PreviousPageButtonLabel => LocalizationHelper.GetString("PreviousPageButtonLabel");
        public string NextPageButtonLabel => LocalizationHelper.GetString("NextPageButtonLabel");
        public string PlayersSearchPlaceholder => $"{AppConstants.DefaultSearchIcon} {LocalizationHelper.GetString("PlayersSearchPlaceholder")}";

        /// <summary>
        /// Gets or sets the search query for filtering players.
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    OnSearchQueryChanged();
                }
            }
        }

        /// <summary>
        /// Gets the localized tooltip for the IdentifierName column.
        /// </summary>
        public string IdentifierNameColumnTooltip => LocalizationHelper.GetString("IdentifierNameColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the DisplayName column.
        /// </summary>
        public string DisplayNameColumnTooltip => LocalizationHelper.GetString("DisplayNameColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the ChessTitle column.
        /// </summary>
        public string ChessTitleColumnTooltip => LocalizationHelper.GetString("ChessTitleColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the CreationDate column.
        /// </summary>
        public string CreationDateColumnTooltip => LocalizationHelper.GetString("CreationDateColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the BulletRating column.
        /// </summary>
        public string BulletRatingColumnTooltip => LocalizationHelper.GetString("BulletRatingColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the BlitzRating column.
        /// </summary>
        public string BlitzRatingColumnTooltip => LocalizationHelper.GetString("BlitzRatingColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the RapidRating column.
        /// </summary>
        public string RapidRatingColumnTooltip => LocalizationHelper.GetString("RapidRatingColumnTooltip");
        /// <summary>
        /// Gets the localized tooltip for the ClassicalRating column.
        /// </summary>
        public string ClassicalRatingColumnTooltip => LocalizationHelper.GetString("ClassicalRatingColumnTooltip");

        /// <summary>
        /// Gets the command to create a new player and navigate to the CreatePlayer view.
        /// </summary>
        public ICommand CreatePlayerCommand { get; }
        public ICommand BulkImportCommand { get; }
        public ICommand BulkExportCommand { get; }
        /// <summary>
        /// Gets the command to select a player and navigate to the Player detail view.
        /// </summary>
        public ICommand SelectPlayerCommand { get; }
        /// <summary>
        /// Gets the command to navigate back to the Dashboard view.
        /// </summary>
        public ICommand BackCommand { get; }
        /// <summary>
        /// Gets the command to navigate to the previous page of players.
        /// </summary>
        public ICommand PreviousPageCommand { get; }
        /// <summary>
        /// Gets the command to navigate to the next page of players.
        /// </summary>
        public ICommand NextPageCommand { get; }
        /// <summary>
        /// Gets the command to change the sort order for players.
        /// </summary>
        public ICommand ChangeSortCommand { get; }

        /// <summary>
        /// Initializes a new instance of the PlayersViewModel class.
        /// </summary>
        /// <param name="logger">The logger for recording application events.</param>
        /// <param name="pageNavigationService">The service for navigating between pages.</param>
        /// <param name="clubService">The service for club data operations.</param>
        /// <param name="playerService">The service for player data operations.</param>
        /// <param name="messageBoxService">The service for displaying message boxes.</param>
        public PlayersViewModel(
            ILogger logger,
            IPageNavigationService pageNavigationService,
            IUISettingsService uiSettingsService,
            IClubService clubService,
            IPlayerService playerService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _uiSettingsService = uiSettingsService;
            _clubService = clubService;
            _playerService = playerService;
            _messageBoxService = messageBoxService;

            CreatePlayerCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<CreatePlayerViewModel>());
            BulkImportCommand = new RelayCommand(async _ => await BulkImportAsync());
            BulkExportCommand = new RelayCommand(async _ => await BulkExportAsync());
            SelectPlayerCommand = new RelayCommand(SelectPlayer);
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
                if (parameters.TryGetValue(SearchQueryParameterKey, out object? savedSearchQueryValue) && savedSearchQueryValue is string savedSearchQuery)
                {
                    _searchQuery = savedSearchQuery;
                    OnPropertyChanged(nameof(SearchQuery));
                }

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
                _searchQuery = string.Empty;
                _currentPage = 1;
                OnPropertyChanged(nameof(SearchQuery));
                OnPropertyChanged(nameof(CurrentPage));
            }

            await LoadPlayersAsync().ConfigureAwait(false);
        }

        private async Task BulkImportAsync()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = ".csv",
                Multiselect = false
            };

            bool? result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }

            string filePath = dlg.FileName;
            try
            {
                var (validPlayers, invalidCount) = await CsvImportExportHelper.ImportPlayersFromCsv(filePath, _playerService, _clubService.CurrentClub);

                int successCount = 0;
                int skippedCount = invalidCount;

                foreach (var player in validPlayers)
                {
                    try
                    {
                        await Task.Run(async () => await _playerService.CreatePlayerAsync(player));
                        successCount++;
                    }
                    catch
                    {
                        skippedCount++;
                    }
                }

                _messageBoxService.ShowInfo(string.Format(LocalizationHelper.GetString("BulkImportResultMessage"), successCount, skippedCount), LocalizationHelper.GetString("Success"));
                await LoadPlayersAsync();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error importing players: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private async Task BulkExportAsync()
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = ".csv",
                AddExtension = true,
                OverwritePrompt = true,
                FileName = "players.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }

            try
            {
                List<Player> players = await _playerService.GetAllPlayersAsync();
                await CsvImportExportHelper.ExportPlayersToCsv(dlg.FileName, players);
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("BulkExportSuccessMessage"), LocalizationHelper.GetString("Success"));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error exporting players: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Loads players asynchronously, calculating total pages and loading the first page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadPlayersAsync()
        {
            try
            {
                int pageSize = Math.Max(1, _uiSettingsService.CurrentAppSettings.PageSize);
                _totalPlayersCount = await _playerService.GetPlayersCountAsync(_searchQuery);
                TotalPages = (int)Math.Ceiling((double)_totalPlayersCount / pageSize);

                if (CurrentPage > TotalPages)
                {
                    _currentPage = Math.Max(1, TotalPages);
                    OnPropertyChanged(nameof(CurrentPage));
                }

                await RefreshPagedPlayersAsync();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading players: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Refreshes the players list for the current page from the database asynchronously.
        /// Fetches only the players needed for the current page and formats them for display.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshPagedPlayersAsync()
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

                _filteredPlayersCount = await _playerService.GetPlayersCountAsync(_searchQuery);
                TotalPages = (int)Math.Ceiling((double)_filteredPlayersCount / pageSize);

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

                List<Player> pagedPlayers = await _playerService.GetPlayersPagedAsync(_currentPage, pageSize, _searchQuery, _sortParameter);

                var displayPlayers = pagedPlayers
                    .Select(p =>
                    {
                        /* Not supporting '?' uncertainty indication for ELO rating system for now due to performance impact */
                        string bulletRating = FormatHelper.FormatRating(p, TimeFormat.Bullet, _clubService.CurrentClub, _playerService, supportEloProvisoryDisplay: false).GetAwaiter().GetResult();
                        string blitzRating = FormatHelper.FormatRating(p, TimeFormat.Blitz, _clubService.CurrentClub, _playerService, supportEloProvisoryDisplay: false).GetAwaiter().GetResult();
                        string rapidRating = FormatHelper.FormatRating(p, TimeFormat.Rapid, _clubService.CurrentClub, _playerService, supportEloProvisoryDisplay: false).GetAwaiter().GetResult();
                        string classicalRating = FormatHelper.FormatRating(p, TimeFormat.Classical, _clubService.CurrentClub, _playerService, supportEloProvisoryDisplay: false).GetAwaiter().GetResult();

                        return new PlayersPageTableItem
                        {
                            IdentifierName = p.IdentifierName,
                            DisplayName = p.DisplayName,
                            ChessTitle = p.ChessTitle,
                            BirthDate = p.BirthDate,
                            CreatedAt = p.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture),
                            BulletRating = bulletRating,
                            BlitzRating = blitzRating,
                            RapidRating = rapidRating,
                            ClassicalRating = classicalRating
                        };
                    })
                    .ToList();

                Players.Clear();
                foreach (PlayersPageTableItem displayPlayer in displayPlayers)
                {
                    Players.Add(displayPlayer);
                }

                OnPropertyChanged(nameof(Players));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(CurrentPageDisplay));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading players: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
            finally
            {
                IsPaginationInProgress = false;
                if (_refreshPending)
                {
                    _ = RefreshPagedPlayersAsync();
                }
            }
        }

        /// <summary>
        /// Handles sorting changes when a column header is clicked.
        /// If the same column is clicked again, the sort direction is reversed.
        /// </summary>
        /// <param name="parameter">The column name to sort by.</param>
        private void ChangeSortColumn(object? parameter)
        {
            string columnName = parameter as string ?? nameof(Player.IdentifierName);

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
                // New column - sort ascending by default
                _sortParameter = new SortParameter(columnName, ListSortDirection.Ascending);
            }

            // Reset to first page and refresh with new sort
            _currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentSortParameter));
            _logger.LogInformation($"PlayersViewModel.ChangeSortColumn: column={columnName}, direction={_sortParameter.SortDirection}");
            RefreshPagedPlayersAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the selection of a player from the players list and navigates to the player detail view.
        /// </summary>
        /// <param name="parameter">The selected PlayerDisplayItem object.</param>
        private void SelectPlayer(object? parameter)
        {
            if (parameter is PlayersPageTableItem playerItem)
            {
                _pageNavigationService.NavigateTo<PlayerViewModel>(
                    pageParameters: new Dictionary<string, object>
                    {
                        { nameof(Player.IdentifierName), playerItem.IdentifierName }
                    },
                    backParameters: new Dictionary<string, object>
                    {
                        { PageNumberParameterKey, CurrentPage },
                        { PageSizeParameterKey, _uiSettingsService.CurrentAppSettings.PageSize },
                        { SearchQueryParameterKey, SearchQuery },
                        { SortParameterKey, new SortParameter(_sortParameter.SortColumn, _sortParameter.SortDirection) }
                    });
            }
        }

        private void OnSearchQueryChanged()
        {
            _currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
            RefreshPagedPlayersAsync().ConfigureAwait(false);
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
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(PlayersPage));
            OnPropertyChanged(nameof(AgeLabel));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(ChessTitleLabel));
            OnPropertyChanged(nameof(CreationDateLabel));
            OnPropertyChanged(nameof(BulletRatingLabel));
            OnPropertyChanged(nameof(BlitzRatingLabel));
            OnPropertyChanged(nameof(RapidRatingLabel));
            OnPropertyChanged(nameof(ClassicalRatingLabel));
            OnPropertyChanged(nameof(CreatePlayerButtonLabel));
            OnPropertyChanged(nameof(BulkImportButtonLabel));
            OnPropertyChanged(nameof(BulkExportButtonLabel));
            OnPropertyChanged(nameof(BackButtonLabel));
            OnPropertyChanged(nameof(PreviousPageButtonLabel));
            OnPropertyChanged(nameof(NextPageButtonLabel));
            OnPropertyChanged(nameof(PlayersSearchPlaceholder));
            OnPropertyChanged(nameof(CurrentPageDisplay));
            OnPropertyChanged(nameof(IdentifierNameColumnTooltip));
            OnPropertyChanged(nameof(DisplayNameColumnTooltip));
            OnPropertyChanged(nameof(ChessTitleColumnTooltip));
            OnPropertyChanged(nameof(CreationDateColumnTooltip));
            OnPropertyChanged(nameof(BulletRatingColumnTooltip));
            OnPropertyChanged(nameof(BlitzRatingColumnTooltip));
            OnPropertyChanged(nameof(RapidRatingColumnTooltip));
            OnPropertyChanged(nameof(ClassicalRatingColumnTooltip));
        }

        public override void OnPageSizeChanged(int _)
        {
            CurrentPage = 1;
            RefreshPagedPlayersAsync().ConfigureAwait(false);
        }
    }
}

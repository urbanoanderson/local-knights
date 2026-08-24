using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Helpers;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Competitions page.
    /// </summary>
    public class CompetitionsViewModel : BaseAppViewModel
    {
        private const string PageNumberParameterKey = "CompetitionsPageNumber";
        private const string PageSizeParameterKey = "CompetitionsPageSize";
        private const string SearchQueryParameterKey = "CompetitionsSearchQuery";
        private const string SortParameterKey = "CompetitionsSortParameter";

        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUISettingsService _uiSettingsService;
        private readonly ICompetitionService _competitionService;
        private readonly IPlayerService _playerService;
        private string _searchQuery = string.Empty;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalCompetitionsCount = 0;
        private bool _isPaginationInProgress;
        private bool _refreshPending;
        private SortParameter _sortParameter = new(nameof(Competition.CreatedAt), ListSortDirection.Descending);

        /// <summary>
        /// Exposes the current sort parameter for UI consumers (e.g., to update sort indicators).
        /// </summary>
        public SortParameter CurrentSortParameter => _sortParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionsViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording service events.</param>
        /// <param name="pageNavigationService">The page navigation service.</param>
        /// <param name="uiSettingsService">The application settings service.</param>
        /// <param name="competitionService">The competition service.</param>
        /// <param name="playerService">The player service.</param>
        public CompetitionsViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IUISettingsService uiSettingsService,
            ICompetitionService competitionService,
            IPlayerService playerService)
        {
            _pageNavigationService = pageNavigationService;
            _uiSettingsService = uiSettingsService;
            _competitionService = competitionService;
            _playerService = playerService;
            LoadCompetitionsCommand = new RelayCommand(async _ => await LoadCompetitionsAsync());
            // Load competitions immediately when ViewModel is created so the view has data
            _ = LoadCompetitionsAsync();
            CreateCompetitionCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<CreateCompetitionViewModel>());
            ViewCompetitionCommand = new RelayCommand(id =>
            {
                if (id is int competitionId)
                {
                    _pageNavigationService.NavigateTo<CompetitionViewModel>(
                        pageParameters: new Dictionary<string, object>
                        {
                            { nameof(Competition.Id), competitionId },
                            { nameof(CompetitionViewModel.SelectedTabIndex), CompetitionViewModel.TabIndexInformation }
                        },
                        backParameters: new Dictionary<string, object>
                        {
                            { PageNumberParameterKey, CurrentPage },
                            { PageSizeParameterKey, _uiSettingsService.CurrentAppSettings.PageSize },
                            { SearchQueryParameterKey, SearchQuery },
                            { SortParameterKey, new SortParameter(_sortParameter.SortColumn, _sortParameter.SortDirection) }
                        });
                }
            });
            BackCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());

            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CanGoToPreviousPage);
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CanGoToNextPage);
            ChangeSortCommand = new RelayCommand(ChangeSortColumn);
        }

        /// <summary>
        /// Sets up the page after transition from another page.
        /// </summary>
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

            await LoadCompetitionsAsync();
        }

        /// <summary>
        /// Gets the collection of competitions for display in the view.
        /// </summary>
        public ObservableCollection<CompetitionsPageTableItem> Competitions { get; } = new ObservableCollection<CompetitionsPageTableItem>();

        public ICommand PreviousPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand ChangeSortCommand { get; private set; }

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
                    _ = RefreshPagedCompetitionsAsync();
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

        /// <summary>
        /// Gets the command to load competitions.
        /// </summary>
        public ICommand LoadCompetitionsCommand { get; }

        /// <summary>
        /// Gets the command to create a new competition.
        /// </summary>
        public ICommand CreateCompetitionCommand { get; }

        /// <summary>
        /// Gets the command to view a competition.
        /// </summary>
        public ICommand ViewCompetitionCommand { get; }

        /// <summary>
        /// Gets the command to go back to dashboard.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Gets the localized page title for the competitions screen.
        /// </summary>
        public string CompetitionsPage => $"{AppConstants.DefaultCompetitionIcon} " + LocalizationHelper.GetString("CompetitionsPage");

        /// <summary>
        /// Gets the localized label for competition ID.
        /// </summary>
        public string CompetitionIdLabel => LocalizationHelper.GetString("CompetitionIdLabel");

        /// <summary>
        /// Gets the localized label for competition creation date.
        /// </summary>
        public string CreationDateLabel => LocalizationHelper.GetString("CreationDateLabel");

        /// <summary>
        /// Gets the localized label for competition title.
        /// </summary>
        public string TitleLabel => LocalizationHelper.GetString("TitleLabel");

        /// <summary>
        /// Gets the localized label for competition type.
        /// </summary>
        public string TypeLabel => LocalizationHelper.GetString("TypeLabel");

        /// <summary>
        /// Gets the localized label for time format.
        /// </summary>
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");

        /// <summary>
        /// Gets the localized label for the rated column.
        /// </summary>
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");

        /// <summary>
        /// Gets the localized label for competition state.
        /// </summary>
        public string StateLabel => LocalizationHelper.GetString("StateLabel");

        /// <summary>
        /// Gets the localized label for competition winner.
        /// </summary>
        public string WinnerLabel => LocalizationHelper.GetString("WinnerLabel");

        /// <summary>
        /// Gets the localized label for the Create button.
        /// </summary>
        public string CreateButtonLabel => LocalizationHelper.GetString("CreateCompetition");

        /// <summary>
        /// Gets the localized label for the Back button.
        /// </summary>
        public string BackButtonLabel => LocalizationHelper.GetString("BackButtonLabel");

        /// <summary>
        /// Gets the localized label for the previous page button.
        /// </summary>
        public string PreviousPageButtonLabel => LocalizationHelper.GetString("PreviousPageButtonLabel");

        /// <summary>
        /// Gets the localized label for the next page button.
        /// </summary>
        public string NextPageButtonLabel => LocalizationHelper.GetString("NextPageButtonLabel");

        /// <summary>
        /// Gets the localized placeholder for the competitions search box.
        /// </summary>
        public string CompetitionsSearchPlaceholder => $"{AppConstants.DefaultSearchIcon} {LocalizationHelper.GetString("CompetitionsSearchPlaceholder")}";

        /// <summary>
        /// Gets or sets the competition title search query.
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
                    _currentPage = 1;
                    OnPropertyChanged(nameof(CurrentPage));
                    _ = RefreshPagedCompetitionsAsync();
                }
            }
        }

        private async Task LoadCompetitionsAsync()
        {
            _totalCompetitionsCount = await _competitionService.GetCompetitionsCountAsync(_searchQuery);

            int pageSize = Math.Max(1, _uiSettingsService.CurrentAppSettings.PageSize);
            TotalPages = (int)Math.Ceiling((double)_totalCompetitionsCount / pageSize);

            if (CurrentPage > TotalPages)
            {
                _currentPage = Math.Max(1, TotalPages);
                OnPropertyChanged(nameof(CurrentPage));
            }

            await RefreshPagedCompetitionsAsync();
        }

        private async Task RefreshPagedCompetitionsAsync()
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
                TotalPages = (int)Math.Ceiling((double)_totalCompetitionsCount / pageSize);

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

                _totalCompetitionsCount = await _competitionService.GetCompetitionsCountAsync(_searchQuery);
                TotalPages = (int)Math.Ceiling((double)_totalCompetitionsCount / pageSize);

                if (_currentPage > TotalPages)
                {
                    _currentPage = Math.Max(1, TotalPages);
                    OnPropertyChanged(nameof(CurrentPage));
                }

                List<Competition> pageItems = await _competitionService.GetCompetitionsPagedAsync(CurrentPage, pageSize, _searchQuery, _sortParameter);

                Competitions.Clear();
                foreach (Competition comp in pageItems)
                {
                    CompetitionsPageTableItem tableItem = new CompetitionsPageTableItem
                    {
                        Id = comp.Id,
                        CreatedAt = comp.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.CurrentCulture),
                        Title = comp.Title,
                        Type = DomainEnumStringConverter.Instance.Convert(comp.Type),
                        TimeFormat = DomainEnumStringConverter.Instance.Convert(comp.TimeFormat),
                        Rated = comp.IsRated ? yesLabel : noLabel,
                        State = DomainEnumStringConverter.Instance.Convert(comp.State)
                    };

                    if (comp.WinnerId != null)
                    {
                        Player winner = await _playerService.GetPlayerByIdentifierAsync(comp.WinnerId);
                        tableItem.Winner = winner?.DisplayName ?? comp.WinnerId ?? string.Empty;
                    }

                    Competitions.Add(tableItem);
                }

                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(CurrentPageDisplay));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception)
            {
                // swallow here; LoadCompetitionsAsync handles errors
            }
            finally
            {
                IsPaginationInProgress = false;
                if (_refreshPending)
                {
                    _ = RefreshPagedCompetitionsAsync();
                }
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

        private void ApplySortToCompetitions()
        {
            // Sorting is applied at the query layer.
        }

        private void ChangeSortColumn(object? parameter)
        {
            string columnName = parameter as string ?? nameof(Competition.CreatedAt);

            // If clicking the same column, reverse the sort direction
            if (_sortParameter.SortColumn == columnName)
            {
                _sortParameter.SortDirection = _sortParameter.SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                // New column - sort ascending by default (descending for CreatedAt for newest first)
                ListSortDirection defaultDirection = columnName == nameof(Competition.CreatedAt)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
                _sortParameter = new SortParameter(columnName, defaultDirection);
            }

            // Sorting is applied during page queries; reset to first page.
            _currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentSortParameter));
            _ = RefreshPagedCompetitionsAsync();
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            OnPropertyChanged(nameof(CompetitionsPage));
            OnPropertyChanged(nameof(CompetitionIdLabel));
            OnPropertyChanged(nameof(CreationDateLabel));
            OnPropertyChanged(nameof(TitleLabel));
            OnPropertyChanged(nameof(TypeLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(StateLabel));
            OnPropertyChanged(nameof(WinnerLabel));
            OnPropertyChanged(nameof(CreateButtonLabel));
            OnPropertyChanged(nameof(BackButtonLabel));
            OnPropertyChanged(nameof(PreviousPageButtonLabel));
            OnPropertyChanged(nameof(NextPageButtonLabel));
            OnPropertyChanged(nameof(CompetitionsSearchPlaceholder));
            OnPropertyChanged(nameof(CurrentPageDisplay));
            RefreshPagedCompetitionsAsync().ConfigureAwait(false);
        }

        public override void OnPageSizeChanged(int _)
        {
            CurrentPage = 1;
            RefreshPagedCompetitionsAsync().ConfigureAwait(false);
        }

    }
}
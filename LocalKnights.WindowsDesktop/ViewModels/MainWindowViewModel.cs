using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using System.Collections.ObjectModel;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Main ViewModel for the desktop application.
    /// Manages navigation between different views, application settings, theme switching, and localization.
    /// Serves as the root ViewModel coordinating the entire application flow.
    /// </summary>
    public class MainWindowViewModel : BaseAppViewModel
    {
        /// <summary>
        /// Logger for the application.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Service for managing page navigation between different views in the application.
        /// </summary>
        private readonly IPageNavigationService _pageNavigationService;

        /// <summary>
        /// Service for managing application settings such as theme and language preferences.
        /// </summary>
        private readonly IUISettingsService _uiSettingsService;

        /// <summary>
        /// Service for managing database access and operations.
        /// </summary>
        private readonly IDatabaseService _databaseService;

        /// <summary>
        /// Service for managing club-related operations such as sign-in and sign-out.
        /// </summary>
        private readonly IClubService _clubService;

        /// <summary>
        /// Service for managing file system operations.
        /// </summary>
        private readonly IFileSystemService _fileSystemService;

        /// <summary>
        /// Service for displaying user-facing message boxes.
        /// </summary>
        private readonly IMessageBoxService _messageBoxService;

        /// <summary>
        /// Gets the command for signing out from the current club.
        /// </summary>
        public ICommand SignOutCommand { get; }

        /// <summary>
        /// Gets the command for navigating to the preferences view.
        /// </summary>
        public ICommand PreferencesCommand { get; }

        /// <summary>
        /// Gets the command for navigating directly to the club dashboard.
        /// </summary>
        public ICommand ClubDashboardCommand { get; }

        /// <summary>
        /// Gets the command to open the application's club database folder in Explorer.
        /// </summary>
        public ICommand OpenDatabaseLocationCommand { get; }

        /// <summary>
        /// Gets the command for switching the UI language to English.
        /// </summary>
        public ICommand SwitchToEnglishCommand { get; private set; }

        /// <summary>
        /// Gets the command for showing the About dialog.
        /// </summary>
        public ICommand AboutCommand { get; }

        /// <summary>
        /// Gets the command for opening the project GitHub page in the user's browser.
        /// </summary>
        public ICommand ProjectPageCommand { get; }

        /// <summary>
        /// Gets the command for closing the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Gets the command for switching the UI language to Portuguese (Brazilian).
        /// </summary>
        public ICommand SwitchToPortugueseCommand { get; private set; }

        /// <summary>
        /// Gets the command for switching the UI language to Spanish.
        /// </summary>
        public ICommand SwitchToSpanishCommand { get; private set; }

        /// <summary>
        /// Gets the command for switching the UI language to French.
        /// </summary>
        public ICommand SwitchToFrenchCommand { get; private set; }

        /// <summary>
        /// Gets the command for switching the UI language to German.
        /// </summary>
        public ICommand SwitchToGermanCommand { get; private set; }

        /// <summary>
        /// Gets the command for switching the UI language to Russian.
        /// </summary>
        public ICommand SwitchToRussianCommand { get; private set; }

        /// <summary>
        /// Gets the command for setting the page size to 10 items.
        /// </summary>
        public ICommand SetPageSize10Command { get; private set; }

        /// <summary>
        /// Gets the command for setting the page size to 50 items.
        /// </summary>
        public ICommand SetPageSize50Command { get; private set; }

        /// <summary>
        /// Gets the command for setting the page size to 100 items.
        /// </summary>
        public ICommand SetPageSize100Command { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger for the application.</param>
        /// <param name="pageNavigationService">The service responsible for handling page navigation.</param>
        /// <param name="uiSettingsService">The application settings service.</param>
        /// <param name="databaseService">The database service for managing data access.</param>
        /// <param name="clubService">The club service.</param>
        /// <param name="fileSystemService">The file system service for managing file operations.</param>
        /// <param name="messageBoxService">The message box service for user notifications.</param>
        public MainWindowViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IUISettingsService uiSettingsService,
            IDatabaseService databaseService,
            IClubService clubService,
            IFileSystemService fileSystemService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _uiSettingsService = uiSettingsService;
            _databaseService = databaseService;
            _clubService = clubService;
            _fileSystemService = fileSystemService;
            _messageBoxService = messageBoxService;

            SignOutCommand = new RelayCommand(_ => ExecuteSignOut(), _ => _clubService.CurrentClub != null);
            ClubDashboardCommand = new RelayCommand(_ => _pageNavigationService.HardNavigateTo<DashboardViewModel>(), _ => CanNavigateToClubDashboard);
            PreferencesCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<ClubPreferencesViewModel>(), _ => _clubService.CurrentClub != null);
            OpenDatabaseLocationCommand = new RelayCommand(_ => ExecuteOpenDatabaseLocation());
            SwitchToEnglishCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("en"));
            SwitchToPortugueseCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("pt-BR"));
            SwitchToSpanishCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("es"));
            SwitchToFrenchCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("fr"));
            SwitchToGermanCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("de"));
            SwitchToRussianCommand = new RelayCommand(_ => _uiSettingsService.SetLanguage("ru"));
            AboutCommand = new RelayCommand(_ => ExecuteAbout());
            ProjectPageCommand = new RelayCommand(_ => ExecuteOpenProjectPage());
            ExitCommand = new RelayCommand(async _ => await ExecuteExitAsync());
            SetPageSize10Command = new RelayCommand(_ => _uiSettingsService.SetPageSize(10));
            SetPageSize50Command = new RelayCommand(_ => _uiSettingsService.SetPageSize(50));
            SetPageSize100Command = new RelayCommand(_ => _uiSettingsService.SetPageSize(100));

            bool defaultDarkMode = _uiSettingsService.IsSystemDarkModeEnabled();
            UISettings settings = _uiSettingsService.Load(defaultDarkMode);

            ApplyLightOrDarkTheme(settings.IsDarkMode);
            OnPageSizeChanged(settings.PageSize);
            OnLanguageChanged(settings.LanguageCode);

            _clubService.OnClubSignIn += (c) =>
            {
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(CanNavigateToClubDashboard));
                CommandManager.InvalidateRequerySuggested();
            };
            _clubService.OnClubSignOut += () =>
            {
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(CanNavigateToClubDashboard));
                CommandManager.InvalidateRequerySuggested();
            };
            _uiSettingsService.OnSetDarkModeToggle += ApplyLightOrDarkTheme;
            _uiSettingsService.OnSetLanguage += OnLanguageChanged;
            _uiSettingsService.OnSetPageSize += OnPageSizeChanged;

            _databaseService.DataAccessStateChanged += (s, e) =>
            {
                bool show = _databaseService.IsReadOperationInProgress || _databaseService.IsWriteOperationInProgress;

                // Ensure UI update happens on UI thread
                if (Application.Current?.Dispatcher != null)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowDataAccess = show));
                else
                    ShowDataAccess = show;

                OnPropertyChanged(nameof(ShowDataAccess));
            };

            _pageNavigationService.OnSetActivePage += (p) => {
                this.CurrentViewModel = p;
                OnPropertyChanged(nameof(CurrentViewModel));
                OnPropertyChanged(nameof(CanNavigateToClubDashboard));
                CommandManager.InvalidateRequerySuggested();
            };
        }

        /// <summary>
        /// Initializes the main view model by setting the initial StartViewModel.
        /// Should be called after the service provider has been configured.
        /// </summary>
        public void Initialize()
        {
            // Set starting page
            _pageNavigationService.HardNavigateTo<StartViewModel>();
        }

        public void OnWindowClosing()
        {
            _clubService.ClubSignOutAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets or sets the current ViewModel being displayed in the main content area.
        /// </summary>
        public BaseAppViewModel CurrentViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data access indicator is shown.
        /// </summary>
        public bool ShowDataAccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application should use dark mode theme.
        /// When changed, applies the theme and persists the setting.
        /// </summary>
        public bool IsDarkMode
        {
            get => _uiSettingsService.CurrentAppSettings.IsDarkMode;
            set
            {
                _uiSettingsService.SetDarkModeToggle(value);
            }
        }

        /// <summary>
        /// Gets or sets the number of items to display per page in paginated views.
        /// Default is 50. Supported values are 10, 50, and 100.
        /// </summary>
        public int PageSize
        {
            get => _uiSettingsService.CurrentAppSettings.PageSize;
            set
            {
                _uiSettingsService.SetPageSize(value);
            }
        }

        /// <summary>
        /// Gets the emoji flag for English-speaking countries.
        /// </summary>
        public string EnglishFlag => "🇺🇸";

        /// <summary>
        /// Gets the emoji flag for Portuguese-speaking countries (Brazil).
        /// </summary>
        public string PortugueseFlag => "🇧🇷";

        /// <summary>
        /// Gets the emoji flag for Spanish-speaking countries.
        /// </summary>
        public string SpanishFlag => "🇪🇸";

        /// <summary>
        /// Gets the emoji flag for French-speaking countries.
        /// </summary>
        public string FrenchFlag => "🇫🇷";

        /// <summary>
        /// Gets the emoji flag for German-speaking countries.
        /// </summary>
        public string GermanFlag => "🇩🇪";

        /// <summary>
        /// Gets the emoji flag for Russian-speaking countries.
        /// </summary>
        public string RussianFlag => "🇷🇺";

        /// <summary>
        /// Gets a value indicating whether English is currently selected as the application language.
        /// </summary>
        public bool IsEnglishSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "en";

        /// <summary>
        /// Gets a value indicating whether Portuguese is currently selected as the application language.
        /// </summary>
        public bool IsPortugueseSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "pt-BR";

        /// <summary>
        /// Gets a value indicating whether Spanish is currently selected as the application language.
        /// </summary>
        public bool IsSpanishSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "es";

        /// <summary>
        /// Gets a value indicating whether French is currently selected as the application language.
        /// </summary>
        public bool IsFrenchSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "fr";

        /// <summary>
        /// Gets a value indicating whether German is currently selected as the application language.
        /// </summary>
        public bool IsGermanSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "de";

        /// <summary>
        /// Gets a value indicating whether Russian is currently selected as the application language.
        /// </summary>
        public bool IsRussianSelected => _uiSettingsService.CurrentAppSettings.LanguageCode == "ru";

        /// <summary>
        /// Gets a value indicating whether the page size is set to 10 items.
        /// </summary>
        public bool IsPageSize10Selected => PageSize == 10;

        /// <summary>
        /// Gets a value indicating whether the page size is set to 50 items.
        /// </summary>
        public bool IsPageSize50Selected => PageSize == 50;

        /// <summary>
        /// Gets a value indicating whether the page size is set to 100 items.
        /// </summary>
        public bool IsPageSize100Selected => PageSize == 100;

        /// <summary>
        /// Gets the window title based on the currently active club.
        /// </summary>
        public string WindowTitle => (_clubService.CurrentClub != null ? $"{AppConstants.AppDisplayName} - {_clubService.CurrentClub.DisplayName}" : AppConstants.AppDisplayName) + (AppConstants.AppVersion.MinorRevision > 0 ? $" ({LocalizationHelper.GetString("BetaTag")})" : string.Empty);

        /// <summary>
        /// Gets the localized menu label for club-related options.
        /// </summary>
        public string ClubMenu => LocalizationHelper.GetString("ClubMenu");

        /// <summary>
        /// Gets the localized label for the sign-out action.
        /// </summary>
        public string SignOut => LocalizationHelper.GetString("SignOut");

        /// <summary>
        /// Gets the localized label for the club dashboard action.
        /// </summary>
        public string ClubDashboard => LocalizationHelper.GetString("ClubDashboard");

        /// <summary>
        /// Gets the localized label for the exit action.
        /// </summary>
        public string Exit => LocalizationHelper.GetString("Exit");

        /// <summary>
        /// Gets the localized menu label for application options.
        /// </summary>
        public string OptionsMenu => LocalizationHelper.GetString("OptionsMenu");

        /// <summary>
        /// Gets the localized label for the open database location action.
        /// </summary>
        public string OpenDatabaseLocation => LocalizationHelper.GetString("OpenDatabaseLocation");

        /// <summary>
        /// Gets the localized menu label for help-related options.
        /// </summary>
        public string HelpMenu => LocalizationHelper.GetString("HelpMenu");

        /// <summary>
        /// Gets the localized label for the about application option.
        /// </summary>
        public string About => LocalizationHelper.GetString("About");

        /// <summary>
        /// Gets the localized label for the project page menu option.
        /// </summary>
        public string ProjectPage => LocalizationHelper.GetString("ProjectPage");

        /// <summary>
        /// Gets the localized label for dark mode option.
        /// </summary>
        public string DarkMode => LocalizationHelper.GetString("DarkMode");

        /// <summary>
        /// Gets the localized label for language selection.
        /// </summary>
        public string ChooseLanguage => LocalizationHelper.GetString("ChooseLanguage");

        /// <summary>
        /// Gets the localized label for English language option.
        /// </summary>
        public string English => LocalizationHelper.GetString("English");

        /// <summary>
        /// Gets the localized label for Portuguese language option.
        /// </summary>
        public string Portuguese => LocalizationHelper.GetString("Portuguese");

        /// <summary>
        /// Gets the localized label for Spanish language option.
        /// </summary>
        public string Spanish => LocalizationHelper.GetString("Spanish");

        /// <summary>
        /// Gets the localized label for French language option.
        /// </summary>
        public string French => LocalizationHelper.GetString("French");

        /// <summary>
        /// Gets the localized label for German language option.
        /// </summary>
        public string German => LocalizationHelper.GetString("German");

        /// <summary>
        /// Gets the localized label for Russian language option.
        /// </summary>
        public string Russian => LocalizationHelper.GetString("Russian");

        /// <summary>
        /// Gets the localized label for the sign-in to club option.
        /// </summary>
        public string SignInToClub => LocalizationHelper.GetString("SignInToClub");

        /// <summary>
        /// Gets the localized label for items per page setting.
        /// </summary>
        public string ItemsPerPage => LocalizationHelper.GetString("ItemsPerPage");

        /// <summary>
        /// Gets the localized label for 10 items per page option.
        /// </summary>
        public string PageSize10 => LocalizationHelper.GetString("PageSize10");

        /// <summary>
        /// Gets the localized label for 50 items per page option.
        /// </summary>
        public string PageSize50 => LocalizationHelper.GetString("PageSize50");

        /// <summary>
        /// Gets the localized label for 100 items per page option.
        /// </summary>
        public string PageSize100 => LocalizationHelper.GetString("PageSize100");

        /// <summary>
        /// Gets a value indicating whether the application can navigate to the club dashboard.
        /// </summary>
        public bool CanNavigateToClubDashboard => _clubService.CurrentClub != null && _pageNavigationService.ActivePage is not DashboardViewModel;

        /// <summary>
        /// Signs out of the current club and returns to the start view.
        /// </summary>
        private async void ExecuteSignOut()
        {
            await _clubService.ClubSignOutAsync();
            _pageNavigationService.HardNavigateTo<StartViewModel>();
        }

        /// <summary>
        /// Opens the club database directory in File Explorer, saving any pending database changes first.
        /// </summary>
        private async void ExecuteOpenDatabaseLocation()
        {
            await _databaseService.SaveChangesAsync();
            string clubsDirectory = _fileSystemService.GetClubsDirectory();
            _fileSystemService.CreateDirectory(clubsDirectory);
            _fileSystemService.OpenDirectory(clubsDirectory);
        }

        /// <summary>
        /// Shows the About dialog with application metadata.
        /// </summary>
        private void ExecuteAbout()
        {
            string message = AppConstants.AppDisplayName + Environment.NewLine +
                            LocalizationHelper.GetStringFormat("AppVersionLabel", "{0}: {1}{2}", $"v{AppConstants.AppVersion.ToString(3)}", Environment.NewLine) +
                            LocalizationHelper.GetStringFormat("AppCreatorLabel", "{0}: {1}{2}", "Anderson Urbano", Environment.NewLine) +
                            LocalizationHelper.GetStringFormat("AppLicenseLabel", "{0}: {1}", LocalizationHelper.GetString("AppLicenseValue"));

            _messageBoxService.ShowInfo(message, LocalizationHelper.GetString("About"));
        }

        /// <summary>
        /// Opens the project's GitHub page in the default browser.
        /// </summary>
        private void ExecuteOpenProjectPage()
        {
            _fileSystemService.OpenUrl("https://github.com/urbanoanderson/local-knights");
        }

        private async Task ExecuteExitAsync()
        {
            await _databaseService.SaveChangesAsync();
            Application.Current.Shutdown();
        }

        private void ApplyLightOrDarkTheme(bool isDarkMode)
        {
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => ApplyLightOrDarkTheme(isDarkMode));
                return;
            }

            OnPropertyChanged(nameof(IsDarkMode));

            // Apply Light/Dark
            Application app = Application.Current;

            if (app == null)
                return;

            Collection<ResourceDictionary> mergedDictionaries = app.Resources.MergedDictionaries;
            Uri themeSource = new Uri($"pack://application:,,,/{AppConstants.AppDisplayName};component/Themes/{(IsDarkMode ? "DarkTheme.xaml" : "LightTheme.xaml")}", UriKind.Absolute);
            ResourceDictionary themeDict = new ResourceDictionary
            {
                Source = themeSource
            };

            List<ResourceDictionary> existingThemeDictionaries = mergedDictionaries
                .Where(d =>
                    d.Source?.OriginalString.EndsWith("Themes/LightTheme.xaml", StringComparison.OrdinalIgnoreCase) == true ||
                    d.Source?.OriginalString.EndsWith("Themes/DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            if (existingThemeDictionaries.Any())
            {
                int firstIndex = mergedDictionaries.IndexOf(existingThemeDictionaries.First());
                mergedDictionaries.Insert(firstIndex, themeDict);
                foreach (ResourceDictionary dict in existingThemeDictionaries)
                {
                    if (dict.Source?.OriginalString != themeSource.OriginalString)
                        mergedDictionaries.Remove(dict);
                }
            }
            else
            {
                mergedDictionaries.Add(themeDict);
            }
        }

        /// <summary>
        /// Handles property change notifications from page size change in app settings to update table page sizes.
        /// </summary>
        public override void OnPageSizeChanged(int pageSize)
        {
            OnPropertyChanged(nameof(PageSize));
            OnPropertyChanged(nameof(IsPageSize10Selected));
            OnPropertyChanged(nameof(IsPageSize50Selected));
            OnPropertyChanged(nameof(IsPageSize100Selected));
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Refresh all localized properties
            OnPropertyChanged(nameof(ClubMenu));
            OnPropertyChanged(nameof(SignOut));
            OnPropertyChanged(nameof(ClubDashboard));
            OnPropertyChanged(nameof(Exit));
            OnPropertyChanged(nameof(OptionsMenu));
            OnPropertyChanged(nameof(OpenDatabaseLocation));
            OnPropertyChanged(nameof(HelpMenu));
            OnPropertyChanged(nameof(About));
            OnPropertyChanged(nameof(ProjectPage));
            OnPropertyChanged(nameof(DarkMode));
            OnPropertyChanged(nameof(ChooseLanguage));
            OnPropertyChanged(nameof(English));
            OnPropertyChanged(nameof(Portuguese));
            OnPropertyChanged(nameof(Spanish));
            OnPropertyChanged(nameof(French));
            OnPropertyChanged(nameof(German));
            OnPropertyChanged(nameof(Russian));
            OnPropertyChanged(nameof(SignInToClub));
            OnPropertyChanged(nameof(ItemsPerPage));
            OnPropertyChanged(nameof(PageSize10));
            OnPropertyChanged(nameof(PageSize50));
            OnPropertyChanged(nameof(PageSize100));
            OnPropertyChanged(nameof(IsEnglishSelected));
            OnPropertyChanged(nameof(IsPortugueseSelected));
            OnPropertyChanged(nameof(IsSpanishSelected));
            OnPropertyChanged(nameof(IsFrenchSelected));
            OnPropertyChanged(nameof(IsGermanSelected));
            OnPropertyChanged(nameof(IsRussianSelected));
            OnPropertyChanged(nameof(WindowTitle));
        }
    }
}
using System.Windows.Input;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Dashboard view that provides navigation to various sections of the application such as Players, Games, and Competitions.
    /// Displays the current club information and provides commands for navigating between different parts of the application.
    /// </summary>
    public class DashboardViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;

        /// <summary>
        /// Gets the localized label for the club dashboard.
        /// </summary>
        public string ClubDashboard => LocalizationHelper.GetString("ClubDashboard") + (_clubService.CurrentClub != null ? $": {_clubService.CurrentClub.DisplayName}" : "");

        /// <summary>
        /// Gets the localized label for "Players" with a player icon emoji.
        /// </summary>
        public string Players => $"{AppConstants.DefaultPlayerIcon} " + LocalizationHelper.GetString("Players");

        /// <summary>
        /// Gets the localized label for "Games" with a flag icon emoji.
        /// </summary>
        public string Games => $"{AppConstants.DefaultGameIcon} " + LocalizationHelper.GetString("Games");

        /// <summary>
        /// Gets the localized label for "Competitions" with a trophy icon emoji.
        /// </summary>
        public string Competitions => $"{AppConstants.DefaultCompetitionIcon} " + LocalizationHelper.GetString("Competitions");

        /// <summary>
        /// Gets the localized label for "Preferences" with a settings icon emoji.
        /// </summary>
        public string Preferences => $"{AppConstants.DefaultPreferencesIcon} " + LocalizationHelper.GetString("Preferences");

        /// <summary>
        /// Gets the command to navigate to the Players view.
        /// </summary>
        public ICommand PlayersCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the Games view.
        /// </summary>
        public ICommand GamesCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the Competitions view.
        /// </summary>
        public ICommand CompetitionsCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the Preferences view.
        /// </summary>
        public ICommand PreferencesCommand { get; }

        /// <summary>
        /// Gets the command to sign out from the current club and navigate to the Start view.
        /// </summary>
        public ICommand SignOutCommand { get; }

        /// <summary>
        /// Initializes a new instance of the DashboardViewModel class.
        /// </summary>
        /// <param name="logger">The logger for the application.</param>
        /// <param name="pageNavigationService">The service responsible for handling page navigation within the application.</param>
        /// <param name="clubService">The service responsible for managing club-related operations.</param>
        public DashboardViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;

            PlayersCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<PlayersViewModel>());
            GamesCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<GamesViewModel>());
            CompetitionsCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<CompetitionsViewModel>());
            PreferencesCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<ClubPreferencesViewModel>());
            SignOutCommand = new RelayCommand(_ => _pageNavigationService.HardNavigateTo<StartViewModel>());
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            OnPropertyChanged(nameof(ClubDashboard));
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(Games));
            OnPropertyChanged(nameof(Competitions));
            OnPropertyChanged(nameof(Preferences));
        }
    }
}
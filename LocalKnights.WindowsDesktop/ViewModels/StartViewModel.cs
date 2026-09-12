using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Start page, handling club selection and sign-in functionality.
    /// </summary>
    public class StartViewModel : BaseAppViewModel
    {
        private string _selectedClub = string.Empty;
        public string SelectedClub
        {
            get => _selectedClub;
            set
            {
                _selectedClub = value;
                OnPropertyChanged(nameof(SelectedClub));
                OnPropertyChanged(nameof(IsSignInEnabled));
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private bool _isSigningIn;
        public bool IsSigningIn
        {
            get => _isSigningIn;
            set
            {
                _isSigningIn = value;
                OnPropertyChanged(nameof(IsSigningIn));
                OnPropertyChanged(nameof(IsSignInEnabled));
                OnPropertyChanged(nameof(SignInButtonText));
            }
        }

        public ObservableCollection<string> Clubs { get; } = new();

        public bool IsSignInEnabled => !string.IsNullOrWhiteSpace(SelectedClub) && !IsSigningIn;

        public string SignInToClub => LocalizationHelper.GetString("SignInToClub");
        public string SignInButtonText => IsSigningIn ? LocalizationHelper.GetString("SigningInButton") : LocalizationHelper.GetString("SignInButton");
        public string CreateClubButton => LocalizationHelper.GetString("PlusButtonLabel");
        public string ClubIdentifierLabel => LocalizationHelper.GetString("ClubIdentifierLabel");
        public string PasswordLabel => LocalizationHelper.GetString("PasswordLabel");
        public string ClubIdentifierTooltip => LocalizationHelper.GetString("ClubIdentifierTooltip");
        public string SignInPasswordTooltip => LocalizationHelper.GetString("SignInPasswordTooltip");

        public ICommand SignInCommand { get; }
        public ICommand CreateClubCommand { get; }

        /// <summary>
        /// Raised when the password input should be cleared from the view.
        /// </summary>
        public event Action? PasswordCleared;

        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IMessageBoxService _messageBoxService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="pageNavigationService">The page navigation service.</param>
        /// <param name="clubService">The club service.</param>
        /// <param name="messageBoxService">The message box service.</param>
        public StartViewModel(
            ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _messageBoxService = messageBoxService;
            SignInCommand = new RelayCommand(SignIn);
            CreateClubCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<CreateClubViewModel>());
        }

        /// <summary>
        /// Sets up the page after transition from another page.
        /// </summary>
        public override async void PageTransitionSetup(IDictionary<string, object> parameters)
        {
            try
            {
                LoadClubs();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in StartViewModel");
                throw;
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            OnPropertyChanged(nameof(SignInToClub));
            OnPropertyChanged(nameof(SignInButtonText));
            OnPropertyChanged(nameof(CreateClubButton));
            OnPropertyChanged(nameof(ClubIdentifierLabel));
            OnPropertyChanged(nameof(PasswordLabel));
            OnPropertyChanged(nameof(ClubIdentifierTooltip));
            OnPropertyChanged(nameof(SignInPasswordTooltip));
        }

        private void LoadClubs()
        {
            Clubs.Clear();
            IEnumerable<string> clubIdentifiers = _clubService.GetExistingClubIdentifiers();
            foreach (string identifier in clubIdentifiers)
            {
                Clubs.Add(identifier);
            }
        }

        private async void SignIn(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(SelectedClub))
                return;

            IsSigningIn = true;
            try
            {
                bool migrationDeclined = false;
                ClubPreferences? clubPreferences = await _clubService.ClubSignInAsync(
                    SelectedClub,
                    Password,
                    (storedVersion, currentVersion) =>
                    {
                        string message = string.Format(
                            LocalizationHelper.GetString("DatabaseMigrationConfirmationMessage"),
                            storedVersion,
                            currentVersion);
                        CustomMessageBoxResult result = _messageBoxService.ShowConfirmation(
                            message,
                            LocalizationHelper.GetString("DatabaseMigrationTitle"));
                        migrationDeclined = result != CustomMessageBoxResult.Yes;
                        return Task.FromResult(!migrationDeclined);
                    });

                if (clubPreferences != null)
                {
                    _pageNavigationService.HardNavigateTo<DashboardViewModel>();
                }
                else if (!migrationDeclined)
                {
                    // Show error message - invalid password or club not found
                    string errorMessage = LocalizationHelper.GetString("InvalidCredentialsMessage");
                    string errorTitle = LocalizationHelper.GetString("SignInFailedTitle");
                    _messageBoxService.ShowError(errorMessage, errorTitle);
                }
            }
            catch (UnsupportedDatabaseSchemaVersionException ex)
            {
                string message = string.Format(
                    LocalizationHelper.GetString("DatabaseSchemaTooNewMessage"),
                    ex.StoredVersion,
                    ex.SupportedVersion);
                _messageBoxService.ShowError(message, LocalizationHelper.GetString("SignInFailedTitle"));
            }
            catch (Exception ex)
            {
                string errorTitle = LocalizationHelper.GetString("Error");
                _messageBoxService.ShowError($"{LocalizationHelper.GetString("SignInFailedMessage")}: {ex.Message}", errorTitle);
            }
            finally
            {
                Password = string.Empty;
                PasswordCleared?.Invoke();
                IsSigningIn = false;
            }
        }
    }
}

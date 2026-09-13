using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for managing club preferences including display name, description, default ratings, and social media links.
    /// Handles validation and persistence of club settings through the ClubService.
    /// </summary>
    public class ClubPreferencesViewModel : BaseAppViewModel
    {
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ClubPreferences _clubPreferences;
        private readonly ILogger _logger;

        private string _displayName = string.Empty;
        private string _description = string.Empty;
        private string _city = string.Empty;
        private string _federation = string.Empty;
        private string _defaultChiefArbiter = string.Empty;
        private double _Rating;
        private string _RatingText = string.Empty;
        private double _minRating;
        private double _maxRating;
        private double _deviation;
        private string _deviationText = string.Empty;
        private double _volatility;
        private string _volatilityText = string.Empty;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;
        private string _newSocialMediaLink = string.Empty;

        /// <summary>
        /// Gets the collection of social media links associated with the club.
        /// </summary>
        public ObservableCollection<string> SocialMediaLinks { get; } = new();

        /// <summary>
        /// Gets or sets the display name of the club.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// Gets or sets the description of the club.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public string Federation
        {
            get => _federation;
            set
            {
                _federation = value?.ToUpperInvariant() ?? string.Empty;
                OnPropertyChanged(nameof(Federation));
            }
        }

        public string DefaultChiefArbiter
        {
            get => _defaultChiefArbiter;
            set
            {
                _defaultChiefArbiter = value;
                OnPropertyChanged(nameof(DefaultChiefArbiter));
            }
        }

        /// <summary>
        /// Gets or sets the rating value for new players in the club.
        /// </summary>
        public double Rating
        {
            get => _Rating;
            set
            {
                _Rating = value;
                RatingText = _Rating.ToString("F0", CultureInfo.CurrentCulture);
                OnPropertyChanged(nameof(Rating));
            }
        }

        /// <summary>
        /// Gets or sets the rating as a text representation for UI binding.
        /// </summary>
        public string RatingText
        {
            get => _RatingText;
            set
            {
                _RatingText = value;
                OnPropertyChanged(nameof(RatingText));
            }
        }

        /// <summary>
        /// Gets or sets the minimum rating allowed for players in the club.
        /// </summary>
        public double MinRating
        {
            get => _minRating;
            set
            {
                _minRating = Math.Clamp(value, CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound);
                OnPropertyChanged(nameof(MinRating));
                OnPropertyChanged(nameof(ClubInitialRatingTooltip));
            }
        }

        /// <summary>
        /// Gets or sets the maximum rating allowed for players in the club.
        /// </summary>
        public double MaxRating
        {
            get => _maxRating;
            set
            {
                _maxRating = Math.Clamp(value, CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound);
                OnPropertyChanged(nameof(MaxRating));
                OnPropertyChanged(nameof(ClubInitialRatingTooltip));
            }
        }

        /// <summary>
        /// Gets or sets the Glicko rating deviation for new players.
        /// </summary>
        public double Deviation
        {
            get => _deviation;
            set
            {
                _deviation = value;
                DeviationText = _deviation.ToString(CultureInfo.CurrentCulture);
                OnPropertyChanged(nameof(Deviation));
            }
        }

        /// <summary>
        /// Gets or sets the Glicko rating deviation as a text representation for UI binding.
        /// </summary>
        public string DeviationText
        {
            get => _deviationText;
            set
            {
                _deviationText = value;
                OnPropertyChanged(nameof(DeviationText));
            }
        }

        /// <summary>
        /// Gets or sets the Glicko rating volatility for new players.
        /// </summary>
        public double Volatility
        {
            get => _volatility;
            set
            {
                _volatility = value;
                VolatilityText = _volatility.ToString(CultureInfo.CurrentCulture);
                OnPropertyChanged(nameof(Volatility));
            }
        }

        /// <summary>
        /// Gets or sets the Glicko rating volatility as a text representation for UI binding.
        /// </summary>
        public string VolatilityText
        {
            get => _volatilityText;
            set
            {
                _volatilityText = value;
                OnPropertyChanged(nameof(VolatilityText));
            }
        }

        private RatingSystem _ratingSystem = RatingSystem.Glicko2;
        public RatingSystem RatingSystem
        {
            get => _ratingSystem;
            set
            {
                _ratingSystem = value;
                OnPropertyChanged(nameof(RatingSystem));
                OnPropertyChanged(nameof(IsGlicko2Selected));
                OnPropertyChanged(nameof(IsEloSelected));
            }
        }

        private int _provisionalGames = CommonConstants.DefaultProvisionalGames;
        public int ProvisionalGames
        {
            get => _provisionalGames;
            set
            {
                _provisionalGames = value;
                OnPropertyChanged(nameof(ProvisionalGames));
            }
        }

        public bool IsGlicko2Selected => RatingSystem == RatingSystem.Glicko2;

        public bool IsEloSelected => RatingSystem == RatingSystem.ELO;

        /// <summary>
        /// Gets or sets the current password for the club (used for validation during password changes).
        /// </summary>
        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        /// <summary>
        /// Gets or sets the new password for the club (encrypted on the database file).
        /// Only used if a password change is requested.
        /// </summary>
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        /// <summary>
        /// Gets or sets the confirmation password for validation during password changes.
        /// Must match NewPassword to confirm the change.
        /// </summary>
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                _confirmNewPassword = value;
                OnPropertyChanged(nameof(ConfirmNewPassword));
            }
        }

        /// <summary>
        /// Raised when password inputs should be cleared from the view.
        /// </summary>
        public event Action? PasswordsCleared;

        /// <summary>
        /// Gets or sets a new social media link to be added to the club's social media list.
        /// </summary>
        public string NewSocialMediaLink
        {
            get => _newSocialMediaLink;
            set
            {
                _newSocialMediaLink = value;
                OnPropertyChanged(nameof(NewSocialMediaLink));
            }
        }

        /// <summary>
        /// Gets the localized label for "Club Preferences".
        /// </summary>
        public string ClubPreferences => $"{AppConstants.DefaultPreferencesIcon} " + LocalizationHelper.GetString("ClubPreferences");
        public string ClubDisplayNameHelp => LocalizationHelper.GetString("ClubDisplayNameHelp");
        public string ClubDescriptionHelp => LocalizationHelper.GetString("ClubDescriptionHelp");
        public string ClubCityHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubCityHelp"), CommonConstants.CityMaxLength);
        public string ClubFederationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubFederationHelp"), CommonConstants.FederationCodeLength);
        public string ClubChiefArbiterHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubChiefArbiterHelp"), CommonConstants.ChiefArbiterMaxLength);
        public string ClubCreatedAtHelp => LocalizationHelper.GetString("ClubCreatedAtHelp");
        public string ClubUpdatedAtHelp => LocalizationHelper.GetString("ClubUpdatedAtHelp");
        public string ClubCreatedAtTooltip => LocalizationHelper.GetString("ClubCreatedAtTooltip");
        public string ClubUpdatedAtTooltip => LocalizationHelper.GetString("ClubUpdatedAtTooltip");

        /// <summary>
        /// Gets the localized label for "Display Name".
        /// </summary>
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");

        /// <summary>
        /// Gets the localized label for "Description".
        /// </summary>
        public string DescriptionLabel => LocalizationHelper.GetString("DescriptionLabel");

        public string CityLabel => LocalizationHelper.GetString("CityLabel");
        public string DefaultChiefArbiterLabel => LocalizationHelper.GetString("DefaultChiefArbiterLabel");
        public string FederationLabel => LocalizationHelper.GetString("FederationLabel");
        public string ClubCityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubCityTooltip"), CommonConstants.CityMaxLength);
        public string ClubChiefArbiterTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubChiefArbiterTooltip"), CommonConstants.ChiefArbiterMaxLength);
        public string ClubFederationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubFederationTooltip"), CommonConstants.FederationCodeLength);

        /// <summary>
        /// Gets the localized label for "Created At".
        /// </summary>
        public string CreatedAtLabel => LocalizationHelper.GetString("CreatedAtLabel");

        /// <summary>
        /// Gets the localized label for "Updated At".
        /// </summary>
        public string UpdatedAtLabel => LocalizationHelper.GetString("UpdatedAtLabel");

        /// <summary>
        /// Gets the club creation date and time formatted for the current culture in local time.
        /// </summary>
        public string CreatedAtText => _clubPreferences.CreatedAt.ToLocalTime().ToString("G", CultureInfo.CurrentCulture);

        /// <summary>
        /// Gets the club last update date and time formatted for the current culture in local time.
        /// </summary>
        public string UpdatedAtText => _clubPreferences.UpdatedAt.ToLocalTime().ToString("G", CultureInfo.CurrentCulture);

        /// <summary>
        /// Gets the localized label for "Rating".
        /// </summary>
        public string RatingLabel => LocalizationHelper.GetString("RatingLabel");

        /// <summary>
        /// Gets the localized label for "Min Rating".
        /// </summary>
        public string MinRatingLabel => LocalizationHelper.GetString("MinRatingLabel");

        /// <summary>
        /// Gets the localized label for "Max Rating".
        /// </summary>
        public string MaxRatingLabel => LocalizationHelper.GetString("MaxRatingLabel");

        /// <summary>
        /// Gets the localized header for the basic information section.
        /// </summary>
        public string BasicInformationHeader => LocalizationHelper.GetString("BasicInformationHeader");

        /// <summary>
        /// Gets the localized header for the rating parameters section.
        /// </summary>
        public string RatingParametersHeader => LocalizationHelper.GetString("RatingParametersHeader");

        /// <summary>
        /// Gets the localized header for the security section.
        /// </summary>
        public string SecurityHeader => LocalizationHelper.GetString("SecurityHeader");

        /// <summary>
        /// Gets the localized label for "Rating System".
        /// </summary>
        public string RatingSystemLabel => LocalizationHelper.GetString("RatingSystemLabel");

        /// <summary>
        /// Gets the localized label for "Deviation".
        /// </summary>
        public string DeviationLabel => LocalizationHelper.GetString("DeviationLabel");

        /// <summary>
        /// Gets the localized label for "Volatility".
        /// </summary>
        public string VolatilityLabel => LocalizationHelper.GetString("VolatilityLabel");

        /// <summary>
        /// Gets the localized label for "Provisional Games".
        /// </summary>
        public string ProvisionalGamesLabel => LocalizationHelper.GetString("ProvisionalGamesLabel");

        /// <summary>
        /// Gets the localized label for "Current Password".
        /// </summary>
        public string CurrentPasswordLabel => LocalizationHelper.GetString("CurrentPasswordLabel");

        /// <summary>
        /// Gets the localized label for "New Password".
        /// </summary>
        public string NewPasswordLabel => LocalizationHelper.GetString("NewPasswordLabel");

        /// <summary>
        /// Gets the localized label for "Confirm New Password".
        /// </summary>
        public string ConfirmNewPasswordLabel => LocalizationHelper.GetString("ConfirmNewPasswordLabel");

        /// <summary>
        /// Gets the localized help text for the security section.
        /// </summary>
        public string SecurityHelp => LocalizationHelper.GetString("SecurityHelp");

        /// <summary>
        /// Gets the localized help text for the rating system selection.
        /// </summary>
        public string ClubRatingSystemHelp => LocalizationHelper.GetString("ClubRatingSystemHelp");

        /// <summary>
        /// Gets the localized help text for provisional games.
        /// </summary>
        public string ClubProvisionalGamesHelp => LocalizationHelper.GetString("ClubProvisionalGamesHelp");
        public string ClubInitialRatingHelp => LocalizationHelper.GetString("ClubInitialRatingHelp");
        public string ClubMinimumRatingHelp => LocalizationHelper.GetString("ClubMinimumRatingHelp");
        public string ClubMaximumRatingHelp => LocalizationHelper.GetString("ClubMaximumRatingHelp");
        public string ClubInitialDeviationHelp => LocalizationHelper.GetString("ClubInitialDeviationHelp");
        public string ClubInitialVolatilityHelp => LocalizationHelper.GetString("ClubInitialVolatilityHelp");

        /// <summary>
        /// Gets the localized label for "Social Media Links".
        /// </summary>
        public string SocialMediaLinksLabel => LocalizationHelper.GetString("SocialMediaLinksLabel");

        /// <summary>
        /// Gets the localized label for "Add" button.
        /// </summary>
        public string AddButtonLabel => LocalizationHelper.GetString("AddButtonLabel");

        /// <summary>
        /// Gets the localized label for "Remove Selected".
        /// </summary>
        public string RemoveSelectedLabel => LocalizationHelper.GetString("RemoveSelectedLabel");

        /// <summary>
        /// Gets the localized label for "Save" button.
        /// </summary>
        public string SaveButtonLabel => LocalizationHelper.GetString("SaveButtonLabel");

        /// <summary>
        /// Gets the localized label for "Delete Club" button.
        /// </summary>
        public string DeleteClubButton => LocalizationHelper.GetString("DeleteClubButton");

        /// <summary>
        /// Gets the localized label for "Cancel" button.
        /// </summary>
        public string CancelLabel => LocalizationHelper.GetString("CancelLabel");

        /// <summary>
        /// Gets the localized help text for password field.
        /// </summary>
        public string ClubCurrentPasswordHelp => LocalizationHelper.GetString("ClubCurrentPasswordHelp");

        /// <summary>
        /// Gets the localized help text for confirm password field.
        /// </summary>
        public string ClubNewPasswordHelp => LocalizationHelper.GetString("ClubNewPasswordHelp");
        public string ClubConfirmNewPasswordHelp => LocalizationHelper.GetString("ClubConfirmNewPasswordHelp");

        /// <summary>
        /// Gets the localized help text for social media field.
        /// </summary>
        public string ClubSocialMediaHelp => LocalizationHelper.GetString("ClubSocialMediaHelp");

        /// <summary>
        /// Gets the localized tooltip for editing display name.
        /// </summary>
        public string ClubDisplayNameTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubDisplayNameTooltip"), CommonConstants.DisplayNameMaxLength);

        /// <summary>
        /// Gets the localized tooltip for editing description.
        /// </summary>
        public string ClubDescriptionTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubDescriptionTooltip"), CommonConstants.DescriptionMaxLength);

        /// <summary>
        /// Gets the localized tooltip for editing rating.
        /// </summary>
        public string ClubInitialRatingTooltip => LocalizationHelper.GetString("ClubInitialRatingTooltip");

        /// <summary>
        /// Gets the localized tooltip for editing minimum rating.
        /// </summary>
        public string ClubMinimumRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubMinimumRatingTooltip"), CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound, CommonConstants.DefaultMinRating);

        /// <summary>
        /// Gets the localized tooltip for editing maximum rating.
        /// </summary>
        public string ClubMaximumRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubMaximumRatingTooltip"), CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound, CommonConstants.DefaultMaxRating);

        /// <summary>
        /// Gets the localized tooltip for editing the rating system.
        /// </summary>
        public string ClubRatingSystemTooltip => LocalizationHelper.GetString("ClubRatingSystemTooltip");

        /// <summary>
        /// Gets the localized tooltip for editing deviation.
        /// </summary>
        public string ClubInitialDeviationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubInitialDeviationTooltip"), CommonConstants.DefaultDeviationLowerBound, CommonConstants.DefaultDeviationUpperBound, CommonConstants.DefaultDeviation);

        /// <summary>
        /// Gets the localized tooltip for editing volatility.
        /// </summary>
        public string ClubInitialVolatilityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubInitialVolatilityTooltip"), CommonConstants.DefaultVolatilityLowerBound, CommonConstants.DefaultVolatilityUpperBound, CommonConstants.DefaultVolatility);

        /// <summary>
        /// Gets the localized tooltip for editing provisional games.
        /// </summary>
        public string ClubProvisionalGamesTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubProvisionalGamesTooltip"), CommonConstants.DefaultProvisionalGamesLowerBound, CommonConstants.DefaultProvisionalGamesUpperBound, CommonConstants.DefaultProvisionalGames);

        /// <summary>
        /// Gets the localized tooltip for editing current password.
        /// </summary>
        public string ClubCurrentPasswordTooltip => LocalizationHelper.GetString("ClubCurrentPasswordTooltip");

        /// <summary>
        /// Gets the localized tooltip for editing new password.
        /// </summary>
        public string ClubNewPasswordTooltip => LocalizationHelper.GetString("ClubNewPasswordTooltip");

        /// <summary>
        /// Gets the localized tooltip for editing confirm new password.
        /// </summary>
        public string ClubConfirmNewPasswordTooltip => LocalizationHelper.GetString("ClubConfirmNewPasswordTooltip");

        /// <summary>
        /// Gets the localized tooltip for editing social media links.
        /// </summary>
        public string ClubSocialMediaTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClubSocialMediaTooltip"), CommonConstants.SocialMediaLinkLimit);

        /// <summary>
        /// Gets the command to save club preferences.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Gets the command to cancel preferences editing and navigate back to dashboard.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets the command to add a new social media link.
        /// </summary>
        public ICommand AddSocialMediaLinkCommand { get; }

        /// <summary>
        /// Gets the command to remove a social media link.
        /// </summary>
        public ICommand RemoveSocialMediaLinkCommand { get; }

        /// <summary>
        /// Gets the command to delete the club.
        /// </summary>
        public ICommand DeleteClubCommand { get; }

        /// <summary>
        /// Initializes a new instance of the ClubPreferencesViewModel class.
        /// </summary>
        /// <param name="logger">The logger for recording service events.</param>
        /// <param name="pageNavigationService">The page navigation service.</param>
        /// <param name="clubService">The service for club data operations.</param>
        /// <param name="messageBoxService">The service for displaying message boxes.</param>
        /// <exception cref="InvalidOperationException">Thrown when no club preferences are currently loaded.</exception>
        public ClubPreferencesViewModel(
            ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _messageBoxService = messageBoxService;
            _clubPreferences = _clubService.CurrentClub;

            DisplayName = _clubPreferences.DisplayName;
            Description = _clubPreferences.Description ?? string.Empty;
            City = _clubPreferences.City ?? string.Empty;
            Federation = _clubPreferences.Federation ?? string.Empty;
            DefaultChiefArbiter = _clubPreferences.DefaultChiefArbiter ?? string.Empty;
            Rating = _clubPreferences.Rating;
            RatingText = _clubPreferences.Rating.ToString("F0", CultureInfo.CurrentCulture);
            MinRating = _clubPreferences.MinRating;
            MaxRating = _clubPreferences.MaxRating;
            Deviation = _clubPreferences.Deviation;
            DeviationText = _clubPreferences.Deviation.ToString(CultureInfo.CurrentCulture);
            Volatility = _clubPreferences.Volatility;
            VolatilityText = _clubPreferences.Volatility.ToString(CultureInfo.CurrentCulture);
            RatingSystem = _clubPreferences.RatingSystem;
            ProvisionalGames = _clubPreferences.ProvisionalGames;

            if (_clubPreferences.SocialMediaLinks is { })
            {
                foreach (string link in _clubPreferences.SocialMediaLinks)
                {
                    SocialMediaLinks.Add(link);
                }
            }

            SaveCommand = new RelayCommand(async _ => await SavePreferencesAsync());
            CancelCommand = new RelayCommand(_ => _pageNavigationService.NavigateTo<DashboardViewModel>());
            AddSocialMediaLinkCommand = new RelayCommand(_ => AddSocialMediaLink());
            RemoveSocialMediaLinkCommand = new RelayCommand(RemoveSocialMediaLink);
            DeleteClubCommand = new RelayCommand(async _ => await DeleteClubAsync());
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(ClubPreferences));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(DescriptionLabel));
            OnPropertyChanged(nameof(CityLabel));
            OnPropertyChanged(nameof(DefaultChiefArbiterLabel));
            OnPropertyChanged(nameof(FederationLabel));
            OnPropertyChanged(nameof(CreatedAtLabel));
            OnPropertyChanged(nameof(UpdatedAtLabel));
            OnPropertyChanged(nameof(CreatedAtText));
            OnPropertyChanged(nameof(UpdatedAtText));
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(MinRatingLabel));
            OnPropertyChanged(nameof(MaxRatingLabel));
            OnPropertyChanged(nameof(BasicInformationHeader));
            OnPropertyChanged(nameof(RatingParametersHeader));
            OnPropertyChanged(nameof(SecurityHeader));
            OnPropertyChanged(nameof(RatingSystemLabel));
            OnPropertyChanged(nameof(DeviationLabel));
            OnPropertyChanged(nameof(VolatilityLabel));
            OnPropertyChanged(nameof(ProvisionalGamesLabel));
            OnPropertyChanged(nameof(CurrentPasswordLabel));
            OnPropertyChanged(nameof(NewPasswordLabel));
            OnPropertyChanged(nameof(ConfirmNewPasswordLabel));
            OnPropertyChanged(nameof(SocialMediaLinksLabel));
            OnPropertyChanged(nameof(AddButtonLabel));
            OnPropertyChanged(nameof(RemoveSelectedLabel));
            OnPropertyChanged(nameof(SaveButtonLabel));
            OnPropertyChanged(nameof(DeleteClubButton));
            OnPropertyChanged(nameof(CancelLabel));
            OnPropertyChanged(nameof(SecurityHelp));
            OnPropertyChanged(nameof(ClubDisplayNameHelp));
            OnPropertyChanged(nameof(ClubDescriptionHelp));
            OnPropertyChanged(nameof(ClubCityHelp));
            OnPropertyChanged(nameof(ClubFederationHelp));
            OnPropertyChanged(nameof(ClubChiefArbiterHelp));
            OnPropertyChanged(nameof(ClubCreatedAtHelp));
            OnPropertyChanged(nameof(ClubUpdatedAtHelp));
            OnPropertyChanged(nameof(ClubRatingSystemHelp));
            OnPropertyChanged(nameof(ClubInitialRatingHelp));
            OnPropertyChanged(nameof(ClubMinimumRatingHelp));
            OnPropertyChanged(nameof(ClubMaximumRatingHelp));
            OnPropertyChanged(nameof(ClubInitialDeviationHelp));
            OnPropertyChanged(nameof(ClubInitialVolatilityHelp));
            OnPropertyChanged(nameof(ClubProvisionalGamesHelp));
            OnPropertyChanged(nameof(ClubCurrentPasswordHelp));
            OnPropertyChanged(nameof(ClubNewPasswordHelp));
            OnPropertyChanged(nameof(ClubConfirmNewPasswordHelp));
            OnPropertyChanged(nameof(ClubSocialMediaHelp));
            OnPropertyChanged(nameof(ClubDisplayNameTooltip));
            OnPropertyChanged(nameof(ClubDescriptionTooltip));
            OnPropertyChanged(nameof(ClubCityTooltip));
            OnPropertyChanged(nameof(ClubFederationTooltip));
            OnPropertyChanged(nameof(ClubChiefArbiterTooltip));
            OnPropertyChanged(nameof(ClubCreatedAtTooltip));
            OnPropertyChanged(nameof(ClubUpdatedAtTooltip));
            OnPropertyChanged(nameof(ClubRatingSystemTooltip));
            OnPropertyChanged(nameof(ClubInitialRatingTooltip));
            OnPropertyChanged(nameof(ClubMinimumRatingTooltip));
            OnPropertyChanged(nameof(ClubMaximumRatingTooltip));
            OnPropertyChanged(nameof(ClubInitialDeviationTooltip));
            OnPropertyChanged(nameof(ClubInitialVolatilityTooltip));
            OnPropertyChanged(nameof(ClubProvisionalGamesTooltip));
            OnPropertyChanged(nameof(ClubCurrentPasswordTooltip));
            OnPropertyChanged(nameof(ClubNewPasswordTooltip));
            OnPropertyChanged(nameof(ClubConfirmNewPasswordTooltip));
            OnPropertyChanged(nameof(ClubSocialMediaTooltip));
        }

        /// <summary>
        /// Validates and saves the club preferences asynchronously. Performs input validation for all fields before saving.
        /// If a new password is provided, validates the current password and ensures new passwords match.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SavePreferencesAsync()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("EnterClubDisplayNameMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            // Password change validation
            bool isChangingPassword = !string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmNewPassword);

            if (isChangingPassword)
            {
                // If attempting password change, validate current password is provided
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("CurrentPasswordRequiredMessage"), LocalizationHelper.GetString("ValidationError"));
                    ClearPasswords();
                    return;
                }

                // Validate current password is correct
                if (!await VerifyCurrentPasswordAsync(_clubPreferences.IdentifierName, CurrentPassword))
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("InvalidCurrentPasswordMessage"), LocalizationHelper.GetString("ValidationError"));
                    ClearPasswords();
                    return;
                }

                // Validate new passwords match
                if (NewPassword != ConfirmNewPassword)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("PasswordsDoNotMatchMessage"), LocalizationHelper.GetString("ValidationError"));
                    ClearPasswords();
                    return;
                }

                // Validate new password is not empty
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("NewPasswordCannotBeEmptyMessage"), LocalizationHelper.GetString("ValidationError"));
                    ClearPasswords();
                    return;
                }
            }

            if (Rating < MinRating || Rating > MaxRating)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("RatingRangeMessage"), MinRating, MaxRating), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (MinRating < CommonConstants.DefaultMinRatingLowerBound || MinRating > CommonConstants.DefaultMinRatingUpperBound)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("MinRatingRangeMessage"), CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (MaxRating < CommonConstants.DefaultMaxRatingLowerBound || MaxRating > CommonConstants.DefaultMaxRatingUpperBound)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("MaxRatingRangeMessage"), CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (MinRating >= MaxRating)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("RatingBoundsOrderMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (RatingSystem == RatingSystem.Glicko2)
            {
                if (Deviation < CommonConstants.DefaultDeviationLowerBound || Deviation > CommonConstants.DefaultDeviationUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("DeviationRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return;
                }

                if (Volatility < CommonConstants.DefaultVolatilityLowerBound || Volatility > CommonConstants.DefaultVolatilityUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("VolatilityRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return;
                }
            }

            if (RatingSystem == RatingSystem.ELO)
            {
                if (ProvisionalGames < CommonConstants.DefaultProvisionalGamesLowerBound || ProvisionalGames > CommonConstants.DefaultProvisionalGamesUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("ProvisionalGamesRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return;
                }
            }

            _clubPreferences.IdentifierName = _clubPreferences.IdentifierName;
            _clubPreferences.DisplayName = DisplayName;
            _clubPreferences.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            _clubPreferences.City = NormalizeOptionalValue(City);
            _clubPreferences.Federation = NormalizeOptionalValue(Federation);
            _clubPreferences.DefaultChiefArbiter = NormalizeOptionalValue(DefaultChiefArbiter);
            _clubPreferences.RatingSystem = RatingSystem;
            _clubPreferences.Rating = Rating;
            _clubPreferences.MinRating = MinRating;
            _clubPreferences.MaxRating = MaxRating;
            _clubPreferences.Deviation = Deviation;
            _clubPreferences.Volatility = Volatility;
            _clubPreferences.ProvisionalGames = ProvisionalGames;
            _clubPreferences.SocialMediaLinks = SocialMediaLinks.ToList();
            _clubPreferences.UpdatedAt = TimeProvider.System.GetUtcNow().DateTime;
            OnPropertyChanged(nameof(UpdatedAtText));

            try
            {
                ClubPreferences? updatedClubPreferences = isChangingPassword
                    ? await _clubService.UpdateClubAsync(_clubPreferences, CurrentPassword, NewPassword)
                    : await _clubService.UpdateClubAsync(_clubPreferences);

                if (updatedClubPreferences != null)
                {
                    _messageBoxService.ShowInfo(LocalizationHelper.GetString("PreferencesSavedMessage"), LocalizationHelper.GetString("Success"));
                    _pageNavigationService.HardNavigateTo<DashboardViewModel>();
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"{LocalizationHelper.GetString("SavePreferencesFailedMessage")} {ex.Message}", LocalizationHelper.GetString("Error"));
            }
            finally
            {
                ClearPasswords();
            }
        }

        private void ClearPasswords()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            PasswordsCleared?.Invoke();
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Verifies that the provided password matches the club's current password by attempting to open a connection.
        /// </summary>
        /// <param name="clubIdentifier">The identifier of the club.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>True if the password is correct; otherwise false.</returns>
        private async Task<bool> VerifyCurrentPasswordAsync(string clubIdentifier, string password)
        {
            try
            {
                // Attempt to open a test connection with the provided password
                // If this succeeds, the password is correct
                await _clubService.VerifyPasswordAsync(clubIdentifier, password);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Password verification failed for club '{clubIdentifier}'. Possible causes: incorrect password, database access issues, or other errors. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Adds a new social media link to the club's social media links collection.
        /// Validates that the link is not empty and that the maximum limit of 5 links is not exceeded.
        /// </summary>
        private void AddSocialMediaLink()
        {
            if (string.IsNullOrWhiteSpace(NewSocialMediaLink))
                return;

            if (SocialMediaLinks.Count >= 5)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("SocialMediaLimitMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            SocialMediaLinks.Add(NewSocialMediaLink.Trim());
            NewSocialMediaLink = string.Empty;
        }

        /// <summary>
        /// Removes a social media link from the club's social media links collection.
        /// </summary>
        /// <param name="parameter">The social media link to remove as a string.</param>
        private void RemoveSocialMediaLink(object? parameter)
        {
            if (parameter is string selectedLink)
            {
                SocialMediaLinks.Remove(selectedLink);
            }
        }

        /// <summary>
        /// Deletes the club asynchronously after confirming with the user and verifying the current password.
        /// Prompts the user for confirmation and requires the correct current password before removing the club from the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DeleteClubAsync()
        {
            var message = LocalizationHelper.GetString("DeleteClubConfirmation");
            CustomMessageBoxResult result = _messageBoxService.ShowConfirmation(message, LocalizationHelper.GetString("Confirm"));
            if (result != CustomMessageBoxResult.Yes)
            {
                ClearPasswords();
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("CurrentPasswordRequiredForDeletionMessage"), LocalizationHelper.GetString("ValidationError"));
                ClearPasswords();
                return;
            }

            try
            {
                await _clubService.DeleteClubAsync(_clubPreferences.IdentifierName, CurrentPassword);
                _pageNavigationService.HardNavigateTo<StartViewModel>();
            }
            catch (InvalidOperationException)
            {
                _messageBoxService.ShowError(LocalizationHelper.GetString("InvalidCurrentPasswordMessage"), LocalizationHelper.GetString("Error"));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"{LocalizationHelper.GetString("DeleteClubFailedMessage")} {ex.Message}", LocalizationHelper.GetString("Error"));
            }
            finally
            {
                ClearPasswords();
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Handles the club creation form with validation for required fields (identifier and display name).
    /// </summary>
    public class CreateClubViewModel : BaseAppViewModel
    {
        private string _identifierName = string.Empty;
        public string IdentifierName
        {
            get => _identifierName;
            set
            {
                _identifierName = value;
                OnPropertyChanged(nameof(IdentifierName));
            }
        }

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string City { get; set; } = string.Empty;

        private string _federation = string.Empty;
        public string Federation
        {
            get => _federation;
            set
            {
                _federation = value?.ToUpperInvariant() ?? string.Empty;
                OnPropertyChanged(nameof(Federation));
            }
        }

        public string DefaultChiefArbiter { get; set; } = string.Empty;

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

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        private double _rating = CommonConstants.DefaultRating;
        public double Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                RatingText = _rating.ToString("F0", CultureInfo.CurrentCulture);
                OnPropertyChanged(nameof(Rating));
            }
        }

        private string _ratingText = CommonConstants.DefaultRating.ToString("F0", CultureInfo.CurrentCulture);
        public string RatingText
        {
            get => _ratingText;
            set
            {
                _ratingText = value;
                OnPropertyChanged(nameof(RatingText));
            }
        }

        private double _minRating = CommonConstants.DefaultMinRating;
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

        private double _maxRating = CommonConstants.DefaultMaxRating;
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

        private double _deviation = CommonConstants.DefaultDeviation;
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

        private string _deviationText = CommonConstants.DefaultDeviation.ToString("F1", CultureInfo.CurrentCulture);
        public string DeviationText
        {
            get => _deviationText;
            set
            {
                _deviationText = value;
                OnPropertyChanged(nameof(DeviationText));
            }
        }

        private double _volatility = CommonConstants.DefaultVolatility;
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

        private string _volatilityText = CommonConstants.DefaultVolatility.ToString("F2", CultureInfo.CurrentCulture);
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

        public ObservableCollection<string> SocialMediaLinks { get; } = new();

        private string _newSocialMediaLink = string.Empty;
        public string NewSocialMediaLink
        {
            get => _newSocialMediaLink;
            set
            {
                _newSocialMediaLink = value;
                OnPropertyChanged(nameof(NewSocialMediaLink));
            }
        }

        public string CreateClubTitle => $"{AppConstants.DefaultPreferencesIcon} " + LocalizationHelper.GetString("CreateClub");
        public string IdentifierNameLabel => LocalizationHelper.GetString("IdentifierNameLabel");
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");
        public string RatingSystemLabel => LocalizationHelper.GetString("RatingSystemLabel");
        public string RatingLabel => LocalizationHelper.GetString("RatingLabel");
        public string MinRatingLabel => LocalizationHelper.GetString("MinRatingLabel");
        public string MaxRatingLabel => LocalizationHelper.GetString("MaxRatingLabel");
        public string DeviationLabel => LocalizationHelper.GetString("DeviationLabel");
        public string VolatilityLabel => LocalizationHelper.GetString("VolatilityLabel");
        public string ProvisionalGamesLabel => LocalizationHelper.GetString("ProvisionalGamesLabel");
        public string DescriptionLabel => LocalizationHelper.GetString("DescriptionLabel");
        public string CityLabel => LocalizationHelper.GetString("CityLabel");
        public string DefaultChiefArbiterLabel => LocalizationHelper.GetString("DefaultChiefArbiterLabel");
        public string FederationLabel => LocalizationHelper.GetString("FederationLabel");
        public string PasswordLabel => LocalizationHelper.GetString("PasswordLabel");
        public string ConfirmPasswordLabel => LocalizationHelper.GetString("ConfirmPasswordLabel");
        public string SocialMediaLinksLabel => LocalizationHelper.GetString("SocialMediaLinksLabel");
        public string AddButtonLabel => LocalizationHelper.GetString("AddButtonLabel");
        public string RemoveSelectedLabel => LocalizationHelper.GetString("RemoveSelectedLabel");
        public string CancelLabel => LocalizationHelper.GetString("CancelLabel");
        public string CreateButtonLabel => LocalizationHelper.GetString("CreateButtonLabel");
        public string RequiredInformationHeader => LocalizationHelper.GetString("RequiredInformationHeader");
        public string RatingParametersHeader => LocalizationHelper.GetString("RatingParametersHeader");
        public string OptionalInformationHeader => LocalizationHelper.GetString("OptionalInformationHeader");
        public string ClubIdentifierNameHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubIdentifierNameHelp"), CommonConstants.IdentifierMaxLength);
        public string ClubDisplayNameHelp => LocalizationHelper.GetString("CreateClubDisplayNameHelp");
        public string ClubRatingSystemHelp => LocalizationHelper.GetString("CreateClubRatingSystemHelp");
        public string ClubInitialRatingHelp => LocalizationHelper.GetString("CreateClubInitialRatingHelp");
        public string ClubInitialDeviationHelp => LocalizationHelper.GetString("CreateClubInitialDeviationHelp");
        public string ClubInitialVolatilityHelp => LocalizationHelper.GetString("CreateClubInitialVolatilityHelp");
        public string ClubProvisionalGamesHelp => LocalizationHelper.GetString("CreateClubProvisionalGamesHelp");
        public string ClubMinimumRatingHelp => LocalizationHelper.GetString("CreateClubMinimumRatingHelp");
        public string ClubMaximumRatingHelp => LocalizationHelper.GetString("CreateClubMaximumRatingHelp");
        public string ClubDescriptionHelp => LocalizationHelper.GetString("CreateClubDescriptionHelp");
        public string ClubCityHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubCityHelp"), CommonConstants.CityMaxLength);
        public string ClubFederationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubFederationHelp"), CommonConstants.FederationCodeLength);
        public string ClubDefaultChiefArbiterHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubDefaultChiefArbiterHelp"), CommonConstants.ChiefArbiterMaxLength);
        public string ClubPasswordHelp => LocalizationHelper.GetString("CreateClubPasswordHelp");
        public string ClubConfirmPasswordHelp => LocalizationHelper.GetString("CreateClubConfirmPasswordHelp");
        public string ClubSocialMediaHelp => LocalizationHelper.GetString("CreateClubSocialMediaHelp");
        public string CreateClub => LocalizationHelper.GetString("CreateClub");
        public string ClubIdentifierNameTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubIdentifierNameTooltip"), CommonConstants.IdentifierMaxLength);
        public string ClubDisplayNameTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubDisplayNameTooltip"), CommonConstants.DisplayNameMaxLength);
        public string ClubInitialRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubInitialRatingTooltip"), MinRating, MaxRating, CommonConstants.DefaultRating);
        public string ClubMinimumRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubMinimumRatingTooltip"), CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound, CommonConstants.DefaultMinRating);
        public string ClubMaximumRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubMaximumRatingTooltip"), CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound, CommonConstants.DefaultMaxRating);
        public string ClubInitialDeviationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubInitialDeviationTooltip"), CommonConstants.DefaultDeviationLowerBound.ToString("F1", CultureInfo.CurrentCulture), CommonConstants.DefaultDeviationUpperBound.ToString("F1", CultureInfo.CurrentCulture), CommonConstants.DefaultDeviation.ToString("F1", CultureInfo.CurrentCulture));
        public string ClubInitialVolatilityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubInitialVolatilityTooltip"), CommonConstants.DefaultVolatilityLowerBound.ToString("F2", CultureInfo.CurrentCulture), CommonConstants.DefaultVolatilityUpperBound.ToString("F2", CultureInfo.CurrentCulture), CommonConstants.DefaultVolatility.ToString("F2", CultureInfo.CurrentCulture));
        public string ClubProvisionalGamesTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubProvisionalGamesTooltip"), CommonConstants.DefaultProvisionalGamesLowerBound, CommonConstants.DefaultProvisionalGamesUpperBound, CommonConstants.DefaultProvisionalGames);
        public string ClubRatingSystemTooltip => LocalizationHelper.GetString("CreateClubRatingSystemTooltip");
        public string ClubDescriptionTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubDescriptionTooltip"), CommonConstants.DescriptionMaxLength);
        public string ClubCityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubCityTooltip"), CommonConstants.CityMaxLength);
        public string ClubFederationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubFederationTooltip"), CommonConstants.FederationCodeLength);
        public string ClubDefaultChiefArbiterTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubDefaultChiefArbiterTooltip"), CommonConstants.ChiefArbiterMaxLength);
        public string ClubPasswordTooltip => LocalizationHelper.GetString("CreateClubPasswordTooltip");
        public string ClubConfirmPasswordTooltip => LocalizationHelper.GetString("CreateClubConfirmPasswordTooltip");
        public string ClubSocialMediaTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateClubSocialMediaTooltip"), CommonConstants.SocialMediaLinkLimit);

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddSocialMediaLinkCommand { get; }
        public ICommand RemoveSocialMediaLinkCommand { get; }

        /// <summary>
        /// Raised when password inputs should be cleared from the view.
        /// </summary>
        public event Action? PasswordsCleared;

        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly TimeProvider _timeProvider;

        public CreateClubViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IMessageBoxService messageBoxService,
            TimeProvider timeProvider)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _messageBoxService = messageBoxService;
            _timeProvider = timeProvider;
            CreateCommand = new RelayCommand(CreateClubAsync);
            CancelCommand = new RelayCommand(_ => _pageNavigationService.HardNavigateTo<StartViewModel>());
            AddSocialMediaLinkCommand = new RelayCommand(AddSocialMediaLink);
            RemoveSocialMediaLinkCommand = new RelayCommand(RemoveSocialMediaLink);
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(CreateClubTitle));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(RatingSystemLabel));
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(MinRatingLabel));
            OnPropertyChanged(nameof(ClubMinimumRatingHelp));
            OnPropertyChanged(nameof(MaxRatingLabel));
            OnPropertyChanged(nameof(ClubMaximumRatingHelp));
            OnPropertyChanged(nameof(DeviationLabel));
            OnPropertyChanged(nameof(VolatilityLabel));
            OnPropertyChanged(nameof(ProvisionalGamesLabel));
            OnPropertyChanged(nameof(DescriptionLabel));
            OnPropertyChanged(nameof(CityLabel));
            OnPropertyChanged(nameof(DefaultChiefArbiterLabel));
            OnPropertyChanged(nameof(FederationLabel));
            OnPropertyChanged(nameof(ClubCityTooltip));
            OnPropertyChanged(nameof(ClubDefaultChiefArbiterTooltip));
            OnPropertyChanged(nameof(ClubFederationTooltip));
            OnPropertyChanged(nameof(PasswordLabel));
            OnPropertyChanged(nameof(ConfirmPasswordLabel));
            OnPropertyChanged(nameof(SocialMediaLinksLabel));
            OnPropertyChanged(nameof(AddButtonLabel));
            OnPropertyChanged(nameof(RemoveSelectedLabel));
            OnPropertyChanged(nameof(CancelLabel));
            OnPropertyChanged(nameof(CreateButtonLabel));
            OnPropertyChanged(nameof(RequiredInformationHeader));
            OnPropertyChanged(nameof(RatingParametersHeader));
            OnPropertyChanged(nameof(OptionalInformationHeader));
            OnPropertyChanged(nameof(ClubIdentifierNameHelp));
            OnPropertyChanged(nameof(ClubDisplayNameHelp));
            OnPropertyChanged(nameof(ClubRatingSystemHelp));
            OnPropertyChanged(nameof(ClubInitialRatingHelp));
            OnPropertyChanged(nameof(ClubInitialDeviationHelp));
            OnPropertyChanged(nameof(ClubInitialVolatilityHelp));
            OnPropertyChanged(nameof(ClubProvisionalGamesHelp));
            OnPropertyChanged(nameof(ClubMinimumRatingHelp));
            OnPropertyChanged(nameof(ClubMaximumRatingHelp));
            OnPropertyChanged(nameof(ClubDescriptionHelp));
            OnPropertyChanged(nameof(ClubCityHelp));
            OnPropertyChanged(nameof(ClubFederationHelp));
            OnPropertyChanged(nameof(ClubDefaultChiefArbiterHelp));
            OnPropertyChanged(nameof(ClubPasswordHelp));
            OnPropertyChanged(nameof(ClubConfirmPasswordHelp));
            OnPropertyChanged(nameof(ClubSocialMediaHelp));
            OnPropertyChanged(nameof(CreateClub));
            OnPropertyChanged(nameof(ClubIdentifierNameTooltip));
            OnPropertyChanged(nameof(ClubDisplayNameTooltip));
            OnPropertyChanged(nameof(ClubInitialRatingTooltip));
            OnPropertyChanged(nameof(ClubMinimumRatingTooltip));
            OnPropertyChanged(nameof(ClubMaximumRatingTooltip));
            OnPropertyChanged(nameof(ClubInitialDeviationTooltip));
            OnPropertyChanged(nameof(ClubInitialVolatilityTooltip));
            OnPropertyChanged(nameof(ClubProvisionalGamesTooltip));
            OnPropertyChanged(nameof(ClubRatingSystemTooltip));
            OnPropertyChanged(nameof(ClubDescriptionTooltip));
            OnPropertyChanged(nameof(ClubPasswordTooltip));
            OnPropertyChanged(nameof(ClubConfirmPasswordTooltip));
            OnPropertyChanged(nameof(ClubSocialMediaTooltip));
        }

        private async void CreateClubAsync(object? parameter)
        {
            if (!ValidateInput())
                return;

            try
            {
                DateTime now = _timeProvider.GetUtcNow().DateTime;
                ClubPreferences clubPreferences = new ClubPreferences
                {
                    CreatedAt = now,
                    UpdatedAt = now,
                    IdentifierName = IdentifierName.ToLower(),
                    DisplayName = DisplayName,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    City = NormalizeOptionalValue(City),
                    Federation = NormalizeOptionalValue(Federation),
                    DefaultChiefArbiter = NormalizeOptionalValue(DefaultChiefArbiter),
                    SocialMediaLinks = SocialMediaLinks.Count > 0 ? SocialMediaLinks.ToList() : null,
                    RatingSystem = RatingSystem,
                    Rating = Rating,
                    MinRating = MinRating,
                    MaxRating = MaxRating,
                    Deviation = Deviation,
                    Volatility = Volatility,
                    ProvisionalGames = ProvisionalGames
                };

                ClubPreferences? createdClubPreferences = await Task.Run(async () => await _clubService.CreateClubAsync(clubPreferences, Password));
                _pageNavigationService.HardNavigateTo<DashboardViewModel>();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Failed to create club: {ex.Message}", "Error");
            }
            finally
            {
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                PasswordsCleared?.Invoke();
            }
        }

        private void AddSocialMediaLink(object? parameter)
        {
            if (!string.IsNullOrWhiteSpace(NewSocialMediaLink) && SocialMediaLinks.Count < CommonConstants.SocialMediaLinkLimit)
            {
                SocialMediaLinks.Add(NewSocialMediaLink);
                NewSocialMediaLink = string.Empty;
            }
        }

        private void RemoveSocialMediaLink(object? parameter)
        {
            if (parameter is string link)
            {
                SocialMediaLinks.Remove(link);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(IdentifierName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("IdentifierNameRequiredMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(IdentifierName, @"^[a-z0-9]+$"))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("IdentifierNameInvalidFormatMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (IdentifierName.Length > CommonConstants.IdentifierMaxLength)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("IdentifierNameMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (_clubService.ClubExists(IdentifierName.ToLower()))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("ClubAlreadyExistsMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("DisplayNameRequiredMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (DisplayName.Length > CommonConstants.DisplayNameMaxLength)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("DisplayNameMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > CommonConstants.DescriptionMaxLength)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("DescriptionMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (Rating < MinRating || Rating > MaxRating)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("RatingRangeMessage"), MinRating, MaxRating), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (MinRating < CommonConstants.DefaultMinRatingLowerBound || MinRating > CommonConstants.DefaultMinRatingUpperBound)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("MinRatingRangeMessage"), CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (MaxRating < CommonConstants.DefaultMaxRatingLowerBound || MaxRating > CommonConstants.DefaultMaxRatingUpperBound)
            {
                _messageBoxService.ShowWarning(string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("MaxRatingRangeMessage"), CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (MinRating >= MaxRating)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("RatingBoundsOrderMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (RatingSystem == RatingSystem.Glicko2)
            {
                if (Deviation < CommonConstants.DefaultDeviationLowerBound || Deviation > CommonConstants.DefaultDeviationUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("DeviationRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return false;
                }

                if (Volatility < CommonConstants.DefaultVolatilityLowerBound || Volatility > CommonConstants.DefaultVolatilityUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("VolatilityRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return false;
                }
            }

            if (RatingSystem == RatingSystem.ELO)
            {
                if (ProvisionalGames < CommonConstants.DefaultProvisionalGamesLowerBound || ProvisionalGames > CommonConstants.DefaultProvisionalGamesUpperBound)
                {
                    _messageBoxService.ShowWarning(LocalizationHelper.GetString("ProvisionalGamesRangeMessage"), LocalizationHelper.GetString("ValidationError"));
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("PasswordRequiredMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (Password != ConfirmPassword)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("PasswordsDoNotMatchMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            return true;
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
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
                OnPropertyChanged(nameof(CreateRatingTooltip));
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
                OnPropertyChanged(nameof(CreateRatingTooltip));
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
        public string CityTooltip => LocalizationHelper.GetString("CityTooltip");
        public string ChiefArbiterTooltip => LocalizationHelper.GetString("ChiefArbiterTooltip");
        public string FederationTooltip => LocalizationHelper.GetString("FederationTooltip");
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
        public string IdentifierNameHelp => LocalizationHelper.GetString("IdentifierNameHelp");
        public string DisplayNameHelp => LocalizationHelper.GetString("DisplayNameHelp");
        public string RatingHelp => LocalizationHelper.GetString("RatingHelp");
        public string DeviationHelp => LocalizationHelper.GetString("DeviationHelp");
        public string VolatilityHelp => LocalizationHelper.GetString("VolatilityHelp");
        public string ProvisionalGamesHelp => LocalizationHelper.GetString("ProvisionalGamesHelp");
        public string RatingSystemHelp => LocalizationHelper.GetString("RatingSystemHelp");
        public string DescriptionHelp => LocalizationHelper.GetString("DescriptionHelp");
        public string PasswordHelp => LocalizationHelper.GetString("PasswordHelp");
        public string ConfirmPasswordHelp => LocalizationHelper.GetString("ConfirmPasswordHelp");
        public string SocialMediaHelp => LocalizationHelper.GetString("SocialMediaHelp");
        public string CreateClub => LocalizationHelper.GetString("CreateClub");
        public string CreateIdentifierNameTooltip => LocalizationHelper.GetString("CreateIdentifierNameTooltip");
        public string CreateDisplayNameTooltip => LocalizationHelper.GetString("CreateDisplayNameTooltip");
        public string CreateRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateRatingTooltip"), MinRating, MaxRating, CommonConstants.DefaultRating);
        public string CreateMinRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateMinRatingTooltip"), CommonConstants.DefaultMinRatingLowerBound, CommonConstants.DefaultMinRatingUpperBound, CommonConstants.DefaultMinRating);
        public string CreateMaxRatingTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateMaxRatingTooltip"), CommonConstants.DefaultMaxRatingLowerBound, CommonConstants.DefaultMaxRatingUpperBound, CommonConstants.DefaultMaxRating);
        public string CreateDeviationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateDeviationTooltip"), CommonConstants.DefaultDeviationLowerBound.ToString("F1", CultureInfo.CurrentCulture), CommonConstants.DefaultDeviationUpperBound.ToString("F1", CultureInfo.CurrentCulture), CommonConstants.DefaultDeviation.ToString("F1", CultureInfo.CurrentCulture));
        public string CreateVolatilityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreateVolatilityTooltip"), CommonConstants.DefaultVolatilityLowerBound.ToString("F2", CultureInfo.CurrentCulture), CommonConstants.DefaultVolatilityUpperBound.ToString("F2", CultureInfo.CurrentCulture), CommonConstants.DefaultVolatility.ToString("F2", CultureInfo.CurrentCulture));
        public string CreateProvisionalGamesTooltip => LocalizationHelper.GetString("CreateProvisionalGamesTooltip");
        public string CreateRatingSystemTooltip => LocalizationHelper.GetString("CreateRatingSystemTooltip");
        public string CreateDescriptionTooltip => LocalizationHelper.GetString("CreateDescriptionTooltip");
        public string CreatePasswordTooltip => LocalizationHelper.GetString("CreatePasswordTooltip");
        public string CreateConfirmPasswordTooltip => LocalizationHelper.GetString("CreateConfirmPasswordTooltip");
        public string CreateSocialMediaTooltip => LocalizationHelper.GetString("CreateSocialMediaTooltip");

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
            OnPropertyChanged(nameof(CreateClubTitle));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(RatingSystemLabel));
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(MinRatingLabel));
            OnPropertyChanged(nameof(MaxRatingLabel));
            OnPropertyChanged(nameof(DeviationLabel));
            OnPropertyChanged(nameof(VolatilityLabel));
            OnPropertyChanged(nameof(ProvisionalGamesLabel));
            OnPropertyChanged(nameof(DescriptionLabel));
            OnPropertyChanged(nameof(CityLabel));
            OnPropertyChanged(nameof(DefaultChiefArbiterLabel));
            OnPropertyChanged(nameof(FederationLabel));
            OnPropertyChanged(nameof(CityTooltip));
            OnPropertyChanged(nameof(ChiefArbiterTooltip));
            OnPropertyChanged(nameof(FederationTooltip));
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
            OnPropertyChanged(nameof(IdentifierNameHelp));
            OnPropertyChanged(nameof(DisplayNameHelp));
            OnPropertyChanged(nameof(RatingHelp));
            OnPropertyChanged(nameof(DeviationHelp));
            OnPropertyChanged(nameof(VolatilityHelp));
            OnPropertyChanged(nameof(ProvisionalGamesHelp));
            OnPropertyChanged(nameof(RatingSystemHelp));
            OnPropertyChanged(nameof(DescriptionHelp));
            OnPropertyChanged(nameof(PasswordHelp));
            OnPropertyChanged(nameof(ConfirmPasswordHelp));
            OnPropertyChanged(nameof(SocialMediaHelp));
            OnPropertyChanged(nameof(CreateClub));
            OnPropertyChanged(nameof(CreateIdentifierNameTooltip));
            OnPropertyChanged(nameof(CreateDisplayNameTooltip));
            OnPropertyChanged(nameof(CreateRatingTooltip));
            OnPropertyChanged(nameof(CreateMinRatingTooltip));
            OnPropertyChanged(nameof(CreateMaxRatingTooltip));
            OnPropertyChanged(nameof(CreateDeviationTooltip));
            OnPropertyChanged(nameof(CreateVolatilityTooltip));
            OnPropertyChanged(nameof(CreateProvisionalGamesTooltip));
            OnPropertyChanged(nameof(CreateRatingSystemTooltip));
            OnPropertyChanged(nameof(CreateDescriptionTooltip));
            OnPropertyChanged(nameof(CreatePasswordTooltip));
            OnPropertyChanged(nameof(CreateConfirmPasswordTooltip));
            OnPropertyChanged(nameof(CreateSocialMediaTooltip));
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
            if (!string.IsNullOrWhiteSpace(NewSocialMediaLink) && SocialMediaLinks.Count < 5)
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

            if (IdentifierName.Length > 50)
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

            if (DisplayName.Length > 100)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("DisplayNameMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
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
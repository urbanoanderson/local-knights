using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Helpers;
using LocalKnights.WindowsDesktop.Services;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Handles the player creation form with validation for player names and rating input.
    /// </summary>
    public class CreatePlayerViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IPlayerService _playerService;
        private readonly IChessDotComApiService _chessDotComApiService;
        private readonly ILichessApiService _lichessApiService;
        private readonly IFideApiService _fideApiService;
        private readonly IMessageBoxService _messageBoxService;

        public string IdentifierName
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(IdentifierName));
            }
        } = string.Empty;

        public string DisplayName
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        } = string.Empty;

        public double BulletRating
        {
            get => field;
            set
            {
                field = Math.Clamp(value, RatingLowerBound, RatingUpperBound);
                OnPropertyChanged(nameof(BulletRating));
            }
        }

        public string BulletDeviationText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(BulletDeviationText));
            }
        } = string.Empty;

        public double BlitzRating
        {
            get => field;
            set
            {
                field = Math.Clamp(value, RatingLowerBound, RatingUpperBound);
                OnPropertyChanged(nameof(BlitzRating));
            }
        }

        public string BlitzDeviationText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(BlitzDeviationText));
            }
        } = string.Empty;

        public double RapidRating
        {
            get => field;
            set
            {
                field = Math.Clamp(value, RatingLowerBound, RatingUpperBound);
                OnPropertyChanged(nameof(RapidRating));
            }
        }

        public string RapidDeviationText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(RapidDeviationText));
            }
        } = string.Empty;

        public double ClassicalRating
        {
            get => field;
            set
            {
                field = Math.Clamp(value, RatingLowerBound, RatingUpperBound);
                OnPropertyChanged(nameof(ClassicalRating));
            }
        }

        public string ClassicalDeviationText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(ClassicalDeviationText));
            }
        } = string.Empty;

        public string LichessUsername
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(LichessUsername));
            }
        } = string.Empty;

        public string ChessComUsername
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(ChessComUsername));
            }
        } = string.Empty;

        public string Email
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Email));
            }
        } = string.Empty;

        public string Phone
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Phone));
            }
        } = string.Empty;

        public DateTime? BirthDate
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(BirthDate));
            }
        }

        public string CbxId
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(CbxId));
            }
        } = string.Empty;

        public string FideId
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(FideId));
            }
        } = string.Empty;

        public string UscfId
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(UscfId));
            }
        } = string.Empty;

        public ChessTitle SelectedChessTitle
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SelectedChessTitle));
            }
        } = ChessTitle.None;

        public GenderType SelectedGender
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SelectedGender));
            }
        } = GenderType.Other;

        private ObservableCollection<ChessTitle> _chessTitles = new(Enum.GetValues(typeof(ChessTitle)).Cast<ChessTitle>());
        private ObservableCollection<GenderType> _genderTypes = new(Enum.GetValues<GenderType>());

        public ObservableCollection<ChessTitle> ChessTitles
        {
            get => _chessTitles;
            private set
            {
                _chessTitles = value;
                OnPropertyChanged(nameof(ChessTitles));
            }
        }

        public ObservableCollection<GenderType> GenderTypes
        {
            get => _genderTypes;
            private set
            {
                _genderTypes = value;
                OnPropertyChanged(nameof(GenderTypes));
            }
        }

        private void RefreshChessTitles()
        {
            // Preserve selection while rebuilding localized items
            var previous = SelectedChessTitle;
            ChessTitles = new ObservableCollection<ChessTitle>(Enum.GetValues(typeof(ChessTitle)).Cast<ChessTitle>());
            SelectedChessTitle = previous;
            OnPropertyChanged(nameof(SelectedChessTitle));
        }

        private void RefreshGenderTypes()
        {
            GenderType previous = SelectedGender;
            GenderTypes = new ObservableCollection<GenderType>(Enum.GetValues<GenderType>());
            SelectedGender = previous;
            OnPropertyChanged(nameof(SelectedGender));
        }

        public string CreatePlayer => $"{AppConstants.DefaultPlayerIcon} " + LocalizationHelper.GetString("CreatePlayer");
        public string CreatePlayerIdentifierHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreatePlayerIdentifierHelp"), CommonConstants.IdentifierMaxLength);
        public string PlayerDisplayNameHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("PlayerDisplayNameHelp"), CommonConstants.DisplayNameMaxLength);
        public string BulletRatingHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("BulletRatingHelp"), RatingLowerBound, RatingUpperBound);
        public string BulletDeviationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("BulletDeviationHelp"), CommonConstants.DefaultDeviation);
        public string BlitzRatingHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("BlitzRatingHelp"), RatingLowerBound, RatingUpperBound);
        public string BlitzDeviationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("BlitzDeviationHelp"), CommonConstants.DefaultDeviation);
        public string RapidRatingHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("RapidRatingHelp"), RatingLowerBound, RatingUpperBound);
        public string RapidDeviationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("RapidDeviationHelp"), CommonConstants.DefaultDeviation);
        public string ClassicalRatingHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClassicalRatingHelp"), RatingLowerBound, RatingUpperBound);
        public string ClassicalDeviationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("ClassicalDeviationHelp"), CommonConstants.DefaultDeviation);
        public string PlayerChessTitleHelp => LocalizationHelper.GetString("PlayerChessTitleHelp");
        public string CreatePlayerChessComUsernameHelp => LocalizationHelper.GetString("CreatePlayerChessComUsernameHelp");
        public string CreatePlayerChessComUsernameTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreatePlayerChessComUsernameTooltip"), CommonConstants.ExternalUsernameMaxLength);
        public string CreatePlayerLichessUsernameHelp => LocalizationHelper.GetString("CreatePlayerLichessUsernameHelp");
        public string CreatePlayerLichessUsernameTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreatePlayerLichessUsernameTooltip"), CommonConstants.ExternalUsernameMaxLength);
        public string CreatePlayerFideIdHelp => LocalizationHelper.GetString("CreatePlayerFideIdHelp");
        public string CreatePlayerFideIdTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CreatePlayerFideIdTooltip"), CommonConstants.FederationIdMaxLength);
        public string PlayerUscfIdHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("PlayerUscfIdHelp"), CommonConstants.FederationIdMaxLength);
        public string PlayerCbxIdHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("PlayerCbxIdHelp"), CommonConstants.FederationIdMaxLength);
        public string PlayerEmailHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("PlayerEmailHelp"), CommonConstants.EmailMaxLength);
        public string PlayerPhoneHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("PlayerPhoneHelp"), CommonConstants.PhoneMaxLength);
        public string PlayerBirthDateHelp => LocalizationHelper.GetString("PlayerBirthDateHelp");
        public string PlayerGenderHelp => LocalizationHelper.GetString("PlayerGenderHelp");
        public string IdentifierNameLabel => LocalizationHelper.GetString("IdentifierNameLabel");
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");
        public string BulletRatingLabel => $"{AppConstants.BulletTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Bullet")}";
        public string BlitzRatingLabel => $"{AppConstants.BlitzTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Blitz")}";
        public string RapidRatingLabel => $"{AppConstants.RapidTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Rapid")}";
        public string ClassicalRatingLabel => $"{AppConstants.ClassicalTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Classical")}";
        public string RatingLabel => $"{LocalizationHelper.GetString("RatingLabel")}:";
        public string DeviationLabel => $"{LocalizationHelper.GetString("DeviationLabel")}:";
        public bool IsDeviationVisible => _clubService.CurrentClub?.RatingSystem == RatingSystem.Glicko2;
        public double RatingLowerBound => _clubService.CurrentClub?.MinRating ?? CommonConstants.DefaultMinRating;
        public double RatingUpperBound => _clubService.CurrentClub?.MaxRating ?? CommonConstants.DefaultMaxRating;
        public string LichessUsernameLabel => LocalizationHelper.GetString("LichessUsernameLabel");
        public string ChessComUsernameLabel => LocalizationHelper.GetString("ChessComUsernameLabel");
        public string EmailLabel => LocalizationHelper.GetString("EmailLabel");
        public string PhoneLabel => LocalizationHelper.GetString("PhoneLabel");
        public string CbxIdLabel => LocalizationHelper.GetString("CbxIdLabel");
        public string FideIdLabel => LocalizationHelper.GetString("FideIdLabel");
        public string UscfIdLabel => LocalizationHelper.GetString("UscfIdLabel");
        public string ChessTitleLabel => LocalizationHelper.GetString("ChessTitleLabel");
        public string GenderLabel => LocalizationHelper.GetString("GenderLabel");
        public string BirthDateLabel => LocalizationHelper.GetString("BirthDateLabel");
        public string CreateButtonLabel => LocalizationHelper.GetString("CreateButtonLabel");
        public string CancelLabel => LocalizationHelper.GetString("CancelLabel");
        public string FetchRatingsLabel => LocalizationHelper.GetString("FetchRatingsLabel");
        public string FetchLichessRatingsResultMessage => LocalizationHelper.GetString("FetchLichessRatingsResultMessage");
        public string FetchFideRatingsResultMessage => LocalizationHelper.GetString("FetchFideRatingsResultMessage");
        public string RequiredFields => LocalizationHelper.GetString("RequiredFields");
        public string OptionalFields => LocalizationHelper.GetString("OptionalFields");
        public string RatingsDescription => LocalizationHelper.GetString("RatingsDescription");
        public string ExternalProfilesLabel => LocalizationHelper.GetString("ExternalProfilesLabel");
        public string ContactInformationSection => LocalizationHelper.GetString("ContactInformationSection");

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FetchChessComRatingsCommand { get; }
        public ICommand FetchLichessRatingsCommand { get; }
        public ICommand FetchFideRatingsCommand { get; }

        public CreatePlayerViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IPlayerService playerService,
            IChessDotComApiService chessDotComApiService,
            ILichessApiService lichessApiService,
            IFideApiService fideApiService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _playerService = playerService;
            _chessDotComApiService = chessDotComApiService;
            _lichessApiService = lichessApiService;
            _fideApiService = fideApiService;
            _messageBoxService = messageBoxService;

            ClubPreferences? currentClub = _clubService.CurrentClub;
            if (currentClub != null)
            {
                string defaultDeviationText = currentClub.Deviation.ToString(CultureInfo.CurrentCulture);

                BulletRating = currentClub.Rating;
                BlitzRating = currentClub.Rating;
                RapidRating = currentClub.Rating;
                ClassicalRating = currentClub.Rating;

                BulletDeviationText = defaultDeviationText;
                BlitzDeviationText = defaultDeviationText;
                RapidDeviationText = defaultDeviationText;
                ClassicalDeviationText = defaultDeviationText;
            }

            CreateCommand = new RelayCommand(async _ => await CreatePlayerAsync());
            CancelCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());
            FetchChessComRatingsCommand = new RelayCommand(async _ => await FetchChessComRatingsAsync());
            FetchLichessRatingsCommand = new RelayCommand(async _ => await FetchLichessRatingsAsync());
            FetchFideRatingsCommand = new RelayCommand(async _ => await FetchFideRatingsAsync());
        }

        private async Task FetchChessComRatingsAsync()
        {
            string chessComUsername = ChessComUsername.Trim();
            IDictionary<TimeFormat, double> ratings = await _chessDotComApiService.GetPlayerRatingsAsync(chessComUsername);

            ApplyFetchedRatings(ratings);

            _messageBoxService.ShowInfo(
                string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizationHelper.GetString("FetchRatingsResultMessage"),
                    ratings.Count,
                    chessComUsername),
                LocalizationHelper.GetString("Success"));
        }

        private async Task FetchLichessRatingsAsync()
        {
            string lichessUsername = LichessUsername.Trim();
            IDictionary<TimeFormat, double> ratings = await _lichessApiService.GetPlayerRatingsAsync(lichessUsername);

            ApplyFetchedRatings(ratings);

            _messageBoxService.ShowInfo(
                string.Format(
                    CultureInfo.CurrentCulture,
                    FetchLichessRatingsResultMessage,
                    ratings.Count,
                    lichessUsername),
                LocalizationHelper.GetString("Success"));
        }

        private async Task FetchFideRatingsAsync()
        {
            string fideId = FideId.Trim();
            IDictionary<TimeFormat, double> ratings = await _fideApiService.GetPlayerRatingsAsync(fideId);

            ApplyFetchedRatings(ratings);

            _messageBoxService.ShowInfo(
                string.Format(
                    CultureInfo.CurrentCulture,
                    FetchFideRatingsResultMessage,
                    ratings.Count,
                    fideId),
                LocalizationHelper.GetString("Success"));
        }

        private void ApplyFetchedRatings(IDictionary<TimeFormat, double> ratings)
        {

            foreach ((TimeFormat timeFormat, double rating) in ratings)
            {
                switch (timeFormat)
                {
                    case TimeFormat.Bullet:
                        BulletRating = rating;
                        break;
                    case TimeFormat.Blitz:
                        BlitzRating = rating;
                        break;
                    case TimeFormat.Rapid:
                        RapidRating = rating;
                        break;
                    case TimeFormat.Classical:
                        ClassicalRating = rating;
                        break;
                }
            }
        }

        private async Task CreatePlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(IdentifierName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("EnterPlayerIdentifierMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(IdentifierName, @"^[a-z0-9]+$"))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("InvalidPlayerIdentifierMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (IdentifierName.Length > 50)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("IdentifierNameMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (await _playerService.PlayerIdentifierExistsAsync(IdentifierName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("PlayerIdentifierExistsMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("EnterPlayerDisplayNameMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (DisplayName.Length > 100)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("DisplayNameMaxLengthMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (BirthDate.HasValue && BirthDate.Value.Date > DateTime.Today)
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("InvalidBirthDateMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            if (!string.IsNullOrWhiteSpace(UscfId) && !System.Text.RegularExpressions.Regex.IsMatch(UscfId, @"^\d{1,12}$"))
            {
                _messageBoxService.ShowWarning(LocalizationHelper.GetString("InvalidUscfIdMessage"), LocalizationHelper.GetString("ValidationError"));
                return;
            }

            try
            {
                double defaultDeviation = _clubService.CurrentClub!.Deviation;
                double volatility = _clubService.CurrentClub!.Volatility;

                Player player = new Player
                {
                    IdentifierName = IdentifierName,
                    DisplayName = DisplayName,
                    LichessUsername = string.IsNullOrWhiteSpace(LichessUsername) ? null : LichessUsername,
                    ChessComUsername = string.IsNullOrWhiteSpace(ChessComUsername) ? null : ChessComUsername,
                    CbxId = string.IsNullOrWhiteSpace(CbxId) ? null : CbxId,
                    FideId = string.IsNullOrWhiteSpace(FideId) ? null : FideId,
                    UscfId = string.IsNullOrWhiteSpace(UscfId) ? null : UscfId,
                    ChessTitle = SelectedChessTitle,
                    Gender = SelectedGender,
                    Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    BirthDate = BirthDate
                };

                // Parse optional ratings
                if (BulletRating > 0)
                {
                    player.BulletRating = new ChessRating
                    {
                        Rating = BulletRating,
                        Deviation = GetNumericValueOrDefault(BulletDeviationText, defaultDeviation),
                        Volatility = volatility
                    };
                }

                if (BlitzRating > 0)
                {
                    player.BlitzRating = new ChessRating
                    {
                        Rating = BlitzRating,
                        Deviation = GetNumericValueOrDefault(BlitzDeviationText, defaultDeviation),
                        Volatility = volatility
                    };
                }

                if (RapidRating > 0)
                {
                    player.RapidRating = new ChessRating
                    {
                        Rating = RapidRating,
                        Deviation = GetNumericValueOrDefault(RapidDeviationText, defaultDeviation),
                        Volatility = volatility
                    };
                }

                if (ClassicalRating > 0)
                {
                    player.ClassicalRating = new ChessRating
                    {
                        Rating = ClassicalRating,
                        Deviation = GetNumericValueOrDefault(ClassicalDeviationText, defaultDeviation),
                        Volatility = volatility
                    };
                }

                await Task.Run(async () => await _playerService.CreatePlayerAsync(player));
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("PlayerCreatedMessage"), LocalizationHelper.GetString("Success"));
                _pageNavigationService.HardNavigateTo<PlayersViewModel>();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error creating player: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(CreatePlayer));
            OnPropertyChanged(nameof(CreatePlayerIdentifierHelp));
            OnPropertyChanged(nameof(PlayerDisplayNameHelp));
            OnPropertyChanged(nameof(BulletRatingHelp));
            OnPropertyChanged(nameof(BulletDeviationHelp));
            OnPropertyChanged(nameof(BlitzRatingHelp));
            OnPropertyChanged(nameof(BlitzDeviationHelp));
            OnPropertyChanged(nameof(RapidRatingHelp));
            OnPropertyChanged(nameof(RapidDeviationHelp));
            OnPropertyChanged(nameof(ClassicalRatingHelp));
            OnPropertyChanged(nameof(ClassicalDeviationHelp));
            OnPropertyChanged(nameof(PlayerChessTitleHelp));
            OnPropertyChanged(nameof(CreatePlayerChessComUsernameHelp));
            OnPropertyChanged(nameof(CreatePlayerChessComUsernameTooltip));
            OnPropertyChanged(nameof(CreatePlayerLichessUsernameHelp));
            OnPropertyChanged(nameof(CreatePlayerLichessUsernameTooltip));
            OnPropertyChanged(nameof(CreatePlayerFideIdHelp));
            OnPropertyChanged(nameof(CreatePlayerFideIdTooltip));
            OnPropertyChanged(nameof(PlayerUscfIdHelp));
            OnPropertyChanged(nameof(PlayerCbxIdHelp));
            OnPropertyChanged(nameof(PlayerEmailHelp));
            OnPropertyChanged(nameof(PlayerPhoneHelp));
            OnPropertyChanged(nameof(PlayerBirthDateHelp));
            OnPropertyChanged(nameof(PlayerGenderHelp));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(BulletRatingLabel));
            OnPropertyChanged(nameof(BlitzRatingLabel));
            OnPropertyChanged(nameof(RapidRatingLabel));
            OnPropertyChanged(nameof(ClassicalRatingLabel));
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(DeviationLabel));
            OnPropertyChanged(nameof(IsDeviationVisible));
            OnPropertyChanged(nameof(RatingLowerBound));
            OnPropertyChanged(nameof(RatingUpperBound));
            OnPropertyChanged(nameof(LichessUsernameLabel));
            OnPropertyChanged(nameof(ChessComUsernameLabel));
            OnPropertyChanged(nameof(EmailLabel));
            OnPropertyChanged(nameof(PhoneLabel));
            OnPropertyChanged(nameof(BirthDateLabel));
            OnPropertyChanged(nameof(CbxIdLabel));
            OnPropertyChanged(nameof(FideIdLabel));
            OnPropertyChanged(nameof(ChessTitleLabel));
            OnPropertyChanged(nameof(GenderLabel));
            OnPropertyChanged(nameof(UscfIdLabel));
            RefreshChessTitles();
            RefreshGenderTypes();
            OnPropertyChanged(nameof(CreateButtonLabel));
            OnPropertyChanged(nameof(CancelLabel));
            OnPropertyChanged(nameof(FetchRatingsLabel));
            OnPropertyChanged(nameof(FetchLichessRatingsResultMessage));
            OnPropertyChanged(nameof(FetchFideRatingsResultMessage));
            OnPropertyChanged(nameof(RequiredFields));
            OnPropertyChanged(nameof(OptionalFields));
            OnPropertyChanged(nameof(RatingsDescription));
            OnPropertyChanged(nameof(ExternalProfilesLabel));
            OnPropertyChanged(nameof(ContactInformationSection));
        }

        private static bool TryGetNumericValue(string rawValue, out double value)
        {
            if (!string.IsNullOrWhiteSpace(rawValue) &&
                double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double parsedValue))
            {
                value = parsedValue;
                return true;
            }

            value = default;
            return false;
        }

        private static double GetNumericValueOrDefault(string rawValue, double fallbackValue)
        {
            return TryGetNumericValue(rawValue, out double value)
                ? value
                : fallbackValue;
        }
    }
}

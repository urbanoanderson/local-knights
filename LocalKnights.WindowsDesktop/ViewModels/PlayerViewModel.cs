using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;
using Microsoft.Extensions.Logging;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Displays player details and ratings in all time formats, allowing editing and deletion operations.
    /// </summary>
    public class PlayerViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly IChessDotComApiService _chessDotComApiService;
        private readonly IPlayerService _playerService;
        private readonly IMessageBoxService _messageBoxService;
        private Player? _currentPlayer;

        private string _displayName = string.Empty;
        private string _lichessUsername = string.Empty;
        private string _chessComUsername = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private DateTime? _birthDate;
        private string _cbxId = string.Empty;
        private string _fideId = string.Empty;
        private string _uscfId = string.Empty;
        private int? _bulletGamesPlayedSinceLastYear = null;
        private int? _blitzGamesPlayedSinceLastYear = null;
        private int? _rapidGamesPlayedSinceLastYear = null;
        private int? _classicalGamesPlayedSinceLastYear = null;
        private ChessTitle _selectedChessTitle = ChessTitle.None;

        public string IdentifierName { get; private set; } = string.Empty;

        private ImageSource _defaultProfileImage = CreateDefaultProfileImage();

        public ImageSource ProfileImage
        {
            get => field;
            private set
            {
                field = value;
                OnPropertyChanged(nameof(ProfileImage));
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string LichessUsername
        {
            get => _lichessUsername;
            set
            {
                _lichessUsername = value;
                OnPropertyChanged(nameof(LichessUsername));
            }
        }

        public string ChessComUsername
        {
            get => _chessComUsername;
            set
            {
                _chessComUsername = value;
                OnPropertyChanged(nameof(ChessComUsername));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                OnPropertyChanged(nameof(BirthDate));
            }
        }

        public string CbxId
        {
            get => _cbxId;
            set
            {
                _cbxId = value;
                OnPropertyChanged(nameof(CbxId));
            }
        }

        public string FideId
        {
            get => _fideId;
            set
            {
                _fideId = value;
                OnPropertyChanged(nameof(FideId));
            }
        }

        public string UscfId
        {
            get => _uscfId;
            set
            {
                _uscfId = value;
                OnPropertyChanged(nameof(UscfId));
            }
        }

        public ChessTitle SelectedChessTitle
        {
            get => _selectedChessTitle;
            set
            {
                _selectedChessTitle = value;
                OnPropertyChanged(nameof(SelectedChessTitle));
            }
        }

        public string CreatedAt { get; private set; } = string.Empty;
        public string BulletRating { get; private set; } = string.Empty;
        public string BlitzRating { get; private set; } = string.Empty;
        public string RapidRating { get; private set; } = string.Empty;
        public string ClassicalRating { get; private set; } = string.Empty;
        public string BulletRatingTooltip { get; private set; } = string.Empty;
        public string BlitzRatingTooltip { get; private set; } = string.Empty;
        public string RapidRatingTooltip { get; private set; } = string.Empty;
        public string ClassicalRatingTooltip { get; private set; } = string.Empty;

        private ObservableCollection<ChessTitle> _chessTitles = new ObservableCollection<ChessTitle>(Enum.GetValues<ChessTitle>());

        public ObservableCollection<ChessTitle> ChessTitles
        {
            get => _chessTitles;
            private set
            {
                _chessTitles = value;
                OnPropertyChanged(nameof(ChessTitles));
            }
        }

        private void RefreshChessTitles()
        {
            ChessTitles = new ObservableCollection<ChessTitle>(Enum.GetValues<ChessTitle>());
        }

        public string PlayerProfile => $"{AppConstants.DefaultPlayerIcon} " + LocalizationHelper.GetString("PlayerProfile");
        public string IdentifierNameLabel => LocalizationHelper.GetString("IdentifierNameLabel");
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");
        public string CreationDateLabel => LocalizationHelper.GetString("CreationDateLabel");
        public string BulletRatingLabel => $"{AppConstants.BulletTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Bullet")}";
        public string BlitzRatingLabel => $"{AppConstants.BlitzTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Blitz")}";
        public string RapidRatingLabel => $"{AppConstants.RapidTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Rapid")}";
        public string ClassicalRatingLabel => $"{AppConstants.ClassicalTimeFormatIcon} {LocalizationHelper.GetString("TimeFormat_Classical")}";
        public string LichessUsernameLabel => LocalizationHelper.GetString("LichessUsernameLabel");
        public string ChessComUsernameLabel => LocalizationHelper.GetString("ChessComUsernameLabel");
        public string EmailLabel => LocalizationHelper.GetString("EmailLabel");
        public string PhoneLabel => LocalizationHelper.GetString("PhoneLabel");
        public string BirthDateLabel => LocalizationHelper.GetString("BirthDateLabel");
        public string BirthDateHelp => LocalizationHelper.GetString("BirthDateHelp");
        public string CbxIdLabel => LocalizationHelper.GetString("CbxIdLabel");
        public string FideIdLabel => LocalizationHelper.GetString("FideIdLabel");
        public string UscfIdLabel => LocalizationHelper.GetString("UscfIdLabel");
        public string ChessTitleLabel => LocalizationHelper.GetString("ChessTitleLabel");
        public string SaveButtonLabel => LocalizationHelper.GetString("SaveButtonLabel");
        public string DeletePlayerButton => LocalizationHelper.GetString("DeletePlayerButton");
        public string CancelLabel => LocalizationHelper.GetString("BackButtonLabel");
        public string PlayerInformationLabel => LocalizationHelper.GetString("PlayerInformationLabel");
        public string RatingsLabel => LocalizationHelper.GetString("RatingsLabel");
        public string ExternalProfilesLabel => LocalizationHelper.GetString("ExternalProfilesLabel");
        public string ContactInformationSection => LocalizationHelper.GetString("ContactInformationSection");
        public string IdentifierNameCannotChangeHelpText => LocalizationHelper.GetString("IdentifierNameCannotChangeHelpText");
        public string Max100CharactersHelpText => LocalizationHelper.GetString("Max100CharactersHelpText");
        public string Max50CharactersHelpText => LocalizationHelper.GetString("Max50CharactersHelpText");
        public string Max20CharactersHelpText => LocalizationHelper.GetString("Max20CharactersHelpText");
        public string NumericMax12CharactersHelpText => LocalizationHelper.GetString("NumericMax12CharactersHelpText");
        public string ChessComProfileImageTooltip => LocalizationHelper.GetString("ChessComProfileImageTooltip");

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public PlayerViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            IChessDotComApiService chessDotComApiService,
            IPlayerService playerService,
            IMessageBoxService messageBoxService)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _chessDotComApiService = chessDotComApiService;
            _playerService = playerService;
            _messageBoxService = messageBoxService;

            SaveCommand = new RelayCommand(async _ => await SavePlayerAsync());
            DeleteCommand = new RelayCommand(async _ => await DeletePlayerAsync());
            BackCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());
        }

        public override async void PageTransitionSetup(IDictionary<string, object> parameters)
        {
            if (parameters.TryGetValue(nameof(Player.IdentifierName), out object? idObj) && idObj is string playerIdentifier)
            {
                await LoadPlayerAsync(playerIdentifier);
            }
            else
            {
                _logger.LogWarning($"No PlayerIdentifier parameter provided for {nameof(PlayerViewModel)}");
            }
        }

        private async Task LoadPlayerAsync(string playerIdentifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerIdentifier))
                {
                    _messageBoxService.ShowError("Invalid player selection", LocalizationHelper.GetString("Error"));
                    BackCommand.Execute(null);
                    return;
                }

                _currentPlayer = await _playerService.GetPlayerByIdentifierAsync(playerIdentifier);
                if (_currentPlayer == null)
                {
                    _messageBoxService.ShowError("Player not found", LocalizationHelper.GetString("Error"));
                    BackCommand.Execute(null);
                    return;
                }

                IdentifierName = _currentPlayer.IdentifierName;
                DisplayName = _currentPlayer.DisplayName;
                LichessUsername = _currentPlayer.LichessUsername ?? string.Empty;
                ChessComUsername = _currentPlayer.ChessComUsername ?? string.Empty;
                Email = _currentPlayer.Email ?? string.Empty;
                Phone = _currentPlayer.Phone ?? string.Empty;
                BirthDate = _currentPlayer.BirthDate;
                CbxId = _currentPlayer.CbxId ?? string.Empty;
                FideId = _currentPlayer.FideId ?? string.Empty;
                UscfId = _currentPlayer.UscfId ?? string.Empty;
                SelectedChessTitle = _currentPlayer.ChessTitle;
                CreatedAt = _currentPlayer.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);

                _ = LoadProfileImageAsync(_currentPlayer.ChessComUsername);

                ClubPreferences clubPreferences = _clubService.CurrentClub;

                // Reload all players for rating formatting
                int bulletPosition = _playerService.GetPlayerTimeFormatPosition(_currentPlayer.IdentifierName, TimeFormat.Bullet);
                int blitzPosition = _playerService.GetPlayerTimeFormatPosition(_currentPlayer.IdentifierName, TimeFormat.Blitz);
                int rapidPosition = _playerService.GetPlayerTimeFormatPosition(_currentPlayer.IdentifierName, TimeFormat.Rapid);
                int classicalPosition = _playerService.GetPlayerTimeFormatPosition(_currentPlayer.IdentifierName, TimeFormat.Classical);

                _bulletGamesPlayedSinceLastYear = null;
                _blitzGamesPlayedSinceLastYear = null;
                _rapidGamesPlayedSinceLastYear = null;
                _classicalGamesPlayedSinceLastYear = null;
                int? provisionalGamesSetting = null;

                if (clubPreferences.RatingSystem == RatingSystem.ELO)
                {
                    _bulletGamesPlayedSinceLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(_currentPlayer.IdentifierName, TimeFormat.Bullet);
                    _blitzGamesPlayedSinceLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(_currentPlayer.IdentifierName, TimeFormat.Blitz);
                    _rapidGamesPlayedSinceLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(_currentPlayer.IdentifierName, TimeFormat.Rapid);
                    _classicalGamesPlayedSinceLastYear = await _playerService.GetPlayerRatedGamesCountSinceLastYear(_currentPlayer.IdentifierName, TimeFormat.Classical);

                    provisionalGamesSetting = clubPreferences.ProvisionalGames;
                }

                BulletRating = FormatHelper.FormatRating(_currentPlayer.BulletRating, clubPreferences.RatingSystem, _bulletGamesPlayedSinceLastYear, provisionalGamesSetting);
                BlitzRating = FormatHelper.FormatRating(_currentPlayer.BlitzRating, clubPreferences.RatingSystem, _blitzGamesPlayedSinceLastYear, provisionalGamesSetting);
                RapidRating = FormatHelper.FormatRating(_currentPlayer.RapidRating, clubPreferences.RatingSystem, _rapidGamesPlayedSinceLastYear, provisionalGamesSetting);
                ClassicalRating = FormatHelper.FormatRating(_currentPlayer.ClassicalRating, clubPreferences.RatingSystem, _classicalGamesPlayedSinceLastYear, provisionalGamesSetting);

                BulletRatingTooltip = CreateRatingTooltip(_currentPlayer.BulletRating, _bulletGamesPlayedSinceLastYear);
                BlitzRatingTooltip = CreateRatingTooltip(_currentPlayer.BlitzRating, _blitzGamesPlayedSinceLastYear);
                RapidRatingTooltip = CreateRatingTooltip(_currentPlayer.RapidRating, _rapidGamesPlayedSinceLastYear);
                ClassicalRatingTooltip = CreateRatingTooltip(_currentPlayer.ClassicalRating, _classicalGamesPlayedSinceLastYear);

                BulletRating = FormatHelper.FormatRatingWithPosition(BulletRating, bulletPosition);
                BlitzRating = FormatHelper.FormatRatingWithPosition(BlitzRating, blitzPosition);
                RapidRating = FormatHelper.FormatRatingWithPosition(RapidRating, rapidPosition);
                ClassicalRating = FormatHelper.FormatRatingWithPosition(ClassicalRating, classicalPosition);

                OnPropertyChanged(nameof(CreatedAt));
                OnPropertyChanged(nameof(BulletRating));
                OnPropertyChanged(nameof(BlitzRating));
                OnPropertyChanged(nameof(RapidRating));
                OnPropertyChanged(nameof(ClassicalRating));
                OnPropertyChanged(nameof(BulletRatingTooltip));
                OnPropertyChanged(nameof(BlitzRatingTooltip));
                OnPropertyChanged(nameof(RapidRatingTooltip));
                OnPropertyChanged(nameof(ClassicalRatingTooltip));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error loading player: {ex.Message}", LocalizationHelper.GetString("Error"));
                BackCommand.Execute(null);
            }
        }

        private async Task SavePlayerAsync()
        {
            if (_currentPlayer == null)
                return;

            string? previousChessComUsername = _currentPlayer.ChessComUsername;

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
                _currentPlayer.DisplayName = DisplayName;
                _currentPlayer.LichessUsername = string.IsNullOrWhiteSpace(LichessUsername) ? null : LichessUsername;
                _currentPlayer.ChessComUsername = string.IsNullOrWhiteSpace(ChessComUsername) ? null : ChessComUsername;
                _currentPlayer.Email = string.IsNullOrWhiteSpace(Email) ? null : Email;
                _currentPlayer.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone;
                _currentPlayer.BirthDate = BirthDate;
                _currentPlayer.CbxId = string.IsNullOrWhiteSpace(CbxId) ? null : CbxId;
                _currentPlayer.FideId = string.IsNullOrWhiteSpace(FideId) ? null : FideId;
                _currentPlayer.UscfId = string.IsNullOrWhiteSpace(UscfId) ? null : UscfId;
                _currentPlayer.ChessTitle = SelectedChessTitle;

                await Task.Run(async () => await _playerService.UpdatePlayerAsync(_currentPlayer));

                if (!string.Equals(previousChessComUsername, _currentPlayer.ChessComUsername, StringComparison.OrdinalIgnoreCase))
                {
                    await LoadProfileImageAsync(_currentPlayer.ChessComUsername);
                }

                _messageBoxService.ShowInfo(LocalizationHelper.GetString("PlayerSavedMessage"), LocalizationHelper.GetString("Success"));
                BackCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error saving player: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        private async Task DeletePlayerAsync()
        {
            if (_currentPlayer == null)
                return;

            CustomMessageBoxResult result = _messageBoxService.ShowConfirmation(
                LocalizationHelper.GetString("DeletePlayerConfirmation"),
                LocalizationHelper.GetString("Confirm"));

            if (result != CustomMessageBoxResult.Yes)
                return;

            try
            {
                await _playerService.DeletePlayerAsync(_currentPlayer.IdentifierName);
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("PlayerDeletedMessage"), LocalizationHelper.GetString("Success"));
                BackCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError($"Error deleting player: {ex.Message}", LocalizationHelper.GetString("Error"));
            }
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            BulletRatingTooltip = CreateRatingTooltip(_currentPlayer.BulletRating, _bulletGamesPlayedSinceLastYear);
            BlitzRatingTooltip = CreateRatingTooltip(_currentPlayer.BlitzRating, _blitzGamesPlayedSinceLastYear);
            RapidRatingTooltip = CreateRatingTooltip(_currentPlayer.RapidRating, _rapidGamesPlayedSinceLastYear);
            ClassicalRatingTooltip = CreateRatingTooltip(_currentPlayer.ClassicalRating, _classicalGamesPlayedSinceLastYear);

            OnPropertyChanged(nameof(PlayerProfile));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(CreationDateLabel));
            OnPropertyChanged(nameof(BulletRatingLabel));
            OnPropertyChanged(nameof(BlitzRatingLabel));
            OnPropertyChanged(nameof(RapidRatingLabel));
            OnPropertyChanged(nameof(ClassicalRatingLabel));
            OnPropertyChanged(nameof(LichessUsernameLabel));
            OnPropertyChanged(nameof(ChessComUsernameLabel));
            OnPropertyChanged(nameof(EmailLabel));
            OnPropertyChanged(nameof(PhoneLabel));
            OnPropertyChanged(nameof(BirthDateLabel));
            OnPropertyChanged(nameof(BirthDateHelp));
            OnPropertyChanged(nameof(ProfileImage));
            OnPropertyChanged(nameof(BulletRatingTooltip));
            OnPropertyChanged(nameof(BlitzRatingTooltip));
            OnPropertyChanged(nameof(RapidRatingTooltip));
            OnPropertyChanged(nameof(ClassicalRatingTooltip));
            OnPropertyChanged(nameof(ChessComProfileImageTooltip));
            // Tooltip and help text updates
            OnPropertyChanged(nameof(IdentifierNameCannotChangeHelpText));
            OnPropertyChanged(nameof(Max100CharactersHelpText));
            OnPropertyChanged(nameof(Max50CharactersHelpText));
            OnPropertyChanged(nameof(Max20CharactersHelpText));
            OnPropertyChanged(nameof(NumericMax12CharactersHelpText));
            OnPropertyChanged(nameof(ContactInformationSection));
            OnPropertyChanged(nameof(ExternalProfilesLabel));
            OnPropertyChanged(nameof(CbxIdLabel));
            OnPropertyChanged(nameof(FideIdLabel));
            OnPropertyChanged(nameof(UscfIdLabel));
            OnPropertyChanged(nameof(ChessTitleLabel));
            RefreshChessTitles();
            OnPropertyChanged(nameof(SaveButtonLabel));
            OnPropertyChanged(nameof(DeletePlayerButton));
            OnPropertyChanged(nameof(CancelLabel));
        }

        private string CreateRatingTooltip(ChessRating rating, int? gamesPlayedSinceLastYear)
        {
            ClubPreferences clubPreferences = _clubService.CurrentClub;

            if (clubPreferences.RatingSystem == RatingSystem.Glicko2)
            {
                return $"{LocalizationHelper.GetString("DeviationLabel")}: {rating.Deviation:F0}";
            }

            if (clubPreferences.RatingSystem == RatingSystem.ELO)
            {
                return gamesPlayedSinceLastYear.HasValue ? $"{LocalizationHelper.GetString("GamesPlayedSinceLastYearLabel")}: {gamesPlayedSinceLastYear.Value}" : string.Empty;
            }

            return string.Empty;
        }

        private async Task LoadProfileImageAsync(string? chessComUsername)
        {
            // Setup default image while loading
            ProfileImage = _defaultProfileImage;

            byte[]? avatarImageBytes = string.IsNullOrWhiteSpace(chessComUsername) ? null
                : await _chessDotComApiService.GetPlayerAvatarImageBytesAsync(chessComUsername);

            ImageSource? loadedProfileImage = CreateImageSource(avatarImageBytes);

            if (loadedProfileImage != null)
            {
                ProfileImage = loadedProfileImage;
            }
        }

        private static ImageSource? CreateImageSource(byte[]? imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            using MemoryStream memoryStream = new MemoryStream(imageBytes);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private static ImageSource CreateDefaultProfileImage()
        {
            Uri resourceUri = new Uri("pack://application:,,,/Resources/Icon/default-profile.png", UriKind.Absolute);
            StreamResourceInfo? resourceStreamInfo = Application.GetResourceStream(resourceUri);
            if (resourceStreamInfo is null)
            {
                return new DrawingImage();
            }

            using MemoryStream memoryStream = new MemoryStream();
            resourceStreamInfo.Stream.CopyTo(memoryStream);
            return CreateImageSource(memoryStream.ToArray()) ?? new DrawingImage();
        }
    }
}

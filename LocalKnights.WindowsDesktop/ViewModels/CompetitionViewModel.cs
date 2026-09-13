using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Helpers.ChessAlgorithms.Pairing;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.WindowsDesktop.Converters;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Services;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Competition page.
    /// </summary>
    public class CompetitionViewModel : BaseAppViewModel
    {
        /// <summary>
        /// Services
        /// </summary>
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private readonly IPlayerService _playerService;
        private readonly IFideApiService _fideApiService;
        private readonly IGameService _gameService;
        private readonly IFileSystemService _fileSystemService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Commands
        /// </summary>
        public ICommand ExportCommand { get; }
        public ICommand ExportToTrf16Command { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand AddParticipantCommand { get; }
        public ICommand RemoveParticipantCommand { get; }
        public ICommand OpenParticipantPlayerCommand { get; }
        public ICommand StartCompetitionCommand { get; }
        public ICommand UndoRoundCommand { get; }
        public ICommand AdvanceRoundCommand { get; }
        public ICommand CompleteCompetitionCommand { get; }
        public ICommand PreviousRoundCommand { get; private set; }
        public ICommand NextRoundCommand { get; private set; }
        public ICommand OpenPairingGameCommand { get; }

        /// <summary>
        /// Localized Texts.
        /// </summary>
        public string CompetitionPage => $"{AppConstants.DefaultCompetitionIcon} " + string.Format(LocalizationHelper.GetString("CompetitionPage"), GetDisplayCompetitionTitle());
        public string CompetitionTitleHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionTitleHelp"), CommonConstants.CompetitionTitleMaxLength);
        public string CompetitionDescriptionHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionDescriptionHelp"), CommonConstants.DescriptionMaxLength);
        public string CompetitionCityHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionCityHelp"), CommonConstants.CityMaxLength);
        public string CompetitionChiefArbiterHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionChiefArbiterHelp"), CommonConstants.ChiefArbiterMaxLength);
        public string CompetitionFederationHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionFederationHelp"), CommonConstants.FederationCodeLength);
        public string CompetitionWinnerHelp => LocalizationHelper.GetString("CompetitionWinnerHelp");
        public string CompetitionTimeFormatHelp => LocalizationHelper.GetString("CompetitionTimeFormatHelp");
        public string CompetitionRatedHelp => LocalizationHelper.GetString("CompetitionRatedHelp");
        public string CompetitionTypeHelp => LocalizationHelper.GetString("CompetitionTypeHelp");
        public string CompetitionRoundsHelp => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("CompetitionRoundsHelp"), CommonConstants.CompetitionRoundsMinimum, CommonConstants.CompetitionRoundsMaximum);
        public string CompetitionDoubleRoundRobinHelp => LocalizationHelper.GetString("CompetitionDoubleRoundRobinHelp");
        public string CompetitionByeValueHelp => LocalizationHelper.GetString("CompetitionByeValueHelp");
        public string CompetitionParticipantSearchHelp => LocalizationHelper.GetString("CompetitionParticipantSearchHelp");
        public string EditCompetitionTitleTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionTitleTooltip"), CommonConstants.CompetitionTitleMaxLength);
        public string EditCompetitionDescriptionTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionDescriptionTooltip"), CommonConstants.DescriptionMaxLength);
        public string EditCompetitionCityTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionCityTooltip"), CommonConstants.CityMaxLength);
        public string EditCompetitionChiefArbiterTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionChiefArbiterTooltip"), CommonConstants.ChiefArbiterMaxLength);
        public string EditCompetitionFederationTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionFederationTooltip"), CommonConstants.FederationCodeLength);
        public string CompetitionWinnerTooltip => LocalizationHelper.GetString("CompetitionWinnerTooltip");
        public string EditCompetitionTimeFormatTooltip => LocalizationHelper.GetString("EditCompetitionTimeFormatTooltip");
        public string EditCompetitionRatedTooltip => LocalizationHelper.GetString("EditCompetitionRatedTooltip");
        public string EditCompetitionTypeTooltip => LocalizationHelper.GetString("EditCompetitionTypeTooltip");
        public string EditCompetitionRoundsTooltip => string.Format(CultureInfo.CurrentCulture, LocalizationHelper.GetString("EditCompetitionRoundsTooltip"), CommonConstants.CompetitionRoundsMinimum, CommonConstants.CompetitionRoundsMaximum, CommonConstants.DefaultCompetitionNumberOfRounds);
        public string EditCompetitionDoubleRoundRobinTooltip => LocalizationHelper.GetString("EditCompetitionDoubleRoundRobinTooltip");
        public string EditCompetitionByeValueTooltip => LocalizationHelper.GetString("EditCompetitionByeValueTooltip");
        public string CompetitionParticipantSearchTooltip => LocalizationHelper.GetString("CompetitionParticipantSearchTooltip");
        public string InformationTabHeader => LocalizationHelper.GetString("InformationTabHeader");
        public string RoundsTabHeader => LocalizationHelper.GetString("RoundsTabHeader");
        public string StandingsTabHeader => LocalizationHelper.GetString("StandingsTabHeader");
        public string TitleLabel => LocalizationHelper.GetString("TitleLabel");
        public string DescriptionLabel => LocalizationHelper.GetString("DescriptionLabel");
        public string CityLabel => LocalizationHelper.GetString("CityLabel");
        public string ChiefArbiterLabel => LocalizationHelper.GetString("ChiefArbiterLabel");
        public string FederationLabel => LocalizationHelper.GetString("FederationLabel");
        public string DisplayNameLabel => LocalizationHelper.GetString("DisplayNameLabel");
        public string IdentifierNameLabel => LocalizationHelper.GetString("IdentifierNameLabel");
        public string TypeLabel => LocalizationHelper.GetString("TypeLabel");
        public string TableNumberLabel => LocalizationHelper.GetString("TableNumberLabel");
        public string GameIdLabel => LocalizationHelper.GetString("GameIdLabel");
        public string WhitePlayerLabel => LocalizationHelper.GetString("WhitePlayerLabel");
        public string ResultLabel => LocalizationHelper.GetString("ResultLabel");
        public string BlackPlayerLabel => LocalizationHelper.GetString("BlackPlayerLabel");
        public string ClassificationLabel => LocalizationHelper.GetString("ClassificationLabel");
        public string ParticipantLabel => LocalizationHelper.GetString("ParticipantLabel");
        public string ScoreLabel => LocalizationHelper.GetString("ScoreLabel");
        public string OldRatingLabel => LocalizationHelper.GetString("OldRatingLabel");
        public string NewRatingLabel => LocalizationHelper.GetString("NewRatingLabel");
        public string PerformanceRatingLabel => LocalizationHelper.GetString("PerformanceRatingLabel");
        public string BuchholzScoreLabel => LocalizationHelper.GetString("BuchholzScoreLabel");
        public string SonnebornBergerScoreLabel => LocalizationHelper.GetString("SonnebornBergerScoreLabel");
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");
        public string NumberOfRoundsLabel => LocalizationHelper.GetString("NumberOfRoundsLabel");
        public string DoubleRoundRobinLabel => LocalizationHelper.GetString("DoubleRoundRobinLabel");
        public string ByeValueLabel => LocalizationHelper.GetString("ByeValueLabel");
        public string WinnerLabel => LocalizationHelper.GetString("WinnerLabel");
        public string SaveButtonLabel => LocalizationHelper.GetString("SaveButtonLabel");
        public string DeleteButtonLabel => LocalizationHelper.GetString("DeleteButtonLabel");
        public string BackButtonLabel => LocalizationHelper.GetString("BackButtonLabel");
        public string StartCompetitionButtonLabel => LocalizationHelper.GetString("StartCompetitionButtonLabel");
        public string AdvanceRoundButtonLabel => LocalizationHelper.GetString("AdvanceRoundButtonLabel");
        public string CompleteCompetitionButtonLabel => LocalizationHelper.GetString("CompleteCompetitionButtonLabel");
        public string UndoRoundButtonLabel => LocalizationHelper.GetString("UndoRoundButtonLabel");
        public string ExportButtonLabel => LocalizationHelper.GetString("ExportButtonLabel");
        public string ExportToExcelLabel => LocalizationHelper.GetString("ExportToExcelLabel");
        public string ExportToTrf16Label => LocalizationHelper.GetString("ExportToTrf16Label");
        public string PreviousPageButtonLabel => LocalizationHelper.GetString("PreviousPageButtonLabel");
        public string NextPageButtonLabel => LocalizationHelper.GetString("NextPageButtonLabel");
        public string AddParticipantButtonLabel => LocalizationHelper.GetString("AddButtonLabel");
        public string RemoveParticipantButtonLabel => LocalizationHelper.GetString("RemoveParticipantButtonLabel");
        public string RatingLabel => LocalizationHelper.GetString("RatingLabel");
        public string CompetitionStateDisplay => Competition != null ? DomainEnumStringConverter.Instance.Convert(Competition.State) : string.Empty;
        public string CompetitionStateBrushKey => Competition?.State switch
        {
            CompetitionState.Starting => "InfoBrush",
            CompetitionState.Ongoing => "WarningBrush",
            CompetitionState.Completed => "SuccessBrush",
            _ => "ForegroundBrush",
        };
        public string WinnerDisplayName => _winner?.DisplayName ?? Competition?.WinnerId ?? string.Empty;
        public string ParticipantsLabel => $"{LocalizationHelper.GetString("ParticipantsLabel")} ({ParticipantTableRows.Count})";
        public string SearchPlaceholder => $"{AppConstants.DefaultSearchIcon} {LocalizationHelper.GetString("PlayersSearchPlaceholder")}";
        public string RoundsPlaceholder => LocalizationHelper.GetString("RoundsPlaceholder");
        public string CurrentRoundLabel => DisplayedRound > 0 ? string.Format(LocalizationHelper.GetString("RoundPairingsLabel"), DisplayedRound) : RoundsPlaceholder;
        public string CurrentRoundLabelDisplay => IsDisplayingCurrentRound ? CurrentRoundLabel + $" ({LocalizationHelper.GetString("CurrentRoundSuffix")})" : CurrentRoundLabel;
        public string RoundIndicator => string.Format(LocalizationHelper.GetString("RoundIndicator"), DisplayedRound, Competition.NumberOfRounds);

        /// <summary>
        /// UI Displayed Properties
        /// </summary>
        private Player? _winner = null;
        public Competition Competition { get; internal set; }

        public int SelectedTabIndex
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                    RefreshSelectedTabElements();
                }
            }
        }

        public CompetitionType SelectedCompetitionType
        {
            get => Competition?.Type ?? CompetitionType.SwissSystemTournament;
            set
            {
                if (Competition != null && Competition.Type != value)
                {
                    if (value == CompetitionType.ChessMatch && Competition.CompetitionParticipants.Count > 2)
                    {
                        CustomMessageBoxResult result = _messageBoxService.Show(
                            LocalizationHelper.GetString("CompetitionTypeMatchParticipantsLimitMessage"),
                            LocalizationHelper.GetString("Confirm"),
                            CustomMessageBoxButton.OKCancel,
                            CustomMessageBoxImage.Warning);

                        if (result != CustomMessageBoxResult.OK)
                        {
                            RefreshInformationTabElements();
                            OnPropertyChanged(nameof(SelectedCompetitionType));
                            return;
                        }

                        var participantsToRemove = Competition.CompetitionParticipants
                            .Skip(2)
                            .Select(participant => participant.ParticipantIdentifierName)
                            .ToList();

                        foreach (string participantIdentifier in participantsToRemove)
                        {
                            Competition = _competitionService.RemoveParticipantAsync(Competition, participantIdentifier).GetAwaiter().GetResult();
                        }
                    }

                    Competition.Type = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    RefreshInformationTabElements();
                }
            }
        }

        public TimeFormat SelectedTimeFormat
        {
            get => Competition?.TimeFormat ?? CommonConstants.DefaultTimeFormat;
            set
            {
                if (Competition != null && Competition.TimeFormat != value)
                {
                    Competition.TimeFormat = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    RefreshInformationTabElements();
                }
            }
        }

        public string CompetitionTitle
        {
            get => Competition?.Title ?? string.Empty;
            set
            {
                if (Competition != null && Competition.Title != value)
                {
                    if (!TryValidateCompetitionTitle(value, out string validatedTitle))
                    {
                        OnPropertyChanged(nameof(CompetitionTitle));
                        return;
                    }

                    Competition.Title = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(CompetitionTitle));
                    OnPropertyChanged(nameof(CompetitionPage));
                }
            }
        }

        public string? CompetitionDescription
        {
            get => Competition?.Description;
            set
            {
                if (Competition != null && Competition.Description != value)
                {
                    Competition.Description = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(CompetitionDescription));
                }
            }
        }

        public string? CompetitionCity
        {
            get => Competition?.City;
            set => UpdateCompetitionMetadata(value, Competition?.City, normalizedValue => Competition.City = normalizedValue, nameof(CompetitionCity));
        }

        public string? CompetitionChiefArbiter
        {
            get => Competition?.ChiefArbiter;
            set => UpdateCompetitionMetadata(value, Competition?.ChiefArbiter, normalizedValue => Competition.ChiefArbiter = normalizedValue, nameof(CompetitionChiefArbiter));
        }

        public string? CompetitionFederation
        {
            get => Competition?.Federation;
            set
            {
                string? normalizedValue = NormalizeOptionalValue(value)?.ToUpperInvariant();
                UpdateCompetitionMetadata(normalizedValue, Competition?.Federation, federation => Competition.Federation = federation, nameof(CompetitionFederation));
            }
        }

        private void UpdateCompetitionMetadata(string? value, string? currentValue, Action<string?> update, string propertyName)
        {
            string? normalizedValue = NormalizeOptionalValue(value);
            if (Competition != null && currentValue != normalizedValue)
            {
                update(normalizedValue);
                Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                OnPropertyChanged(propertyName);
            }
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public bool IsCompetitionRated
        {
            get => Competition?.IsRated ?? false;
            set
            {
                if (Competition != null && Competition.IsRated != value)
                {
                    Competition.IsRated = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(IsCompetitionRated));
                }
            }
        }

        public double CompetitionNumberOfRounds
        {
            get => Competition?.NumberOfRounds ?? 1;
            set
            {
                int roundedValue = (int)Math.Round(value);

                if (Competition != null && Competition.NumberOfRounds != roundedValue)
                {
                    Competition.NumberOfRounds = roundedValue;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(CompetitionNumberOfRounds));
                }
            }
        }

        public bool IsCompetitionDoubleRoundRobin
        {
            get => Competition?.IsDoubleRoundRobin ?? false;
            set
            {
                if (Competition != null && Competition.IsDoubleRoundRobin != value)
                {
                    Competition.IsDoubleRoundRobin = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(IsCompetitionDoubleRoundRobin));
                }
            }
        }

        public double? CompetitionByeValue
        {
            get => Competition?.ByeValue;
            set
            {
                if (Competition != null && Competition.ByeValue != value)
                {
                    Competition.ByeValue = value;
                    Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
                    OnPropertyChanged(nameof(CompetitionByeValue));
                }
            }
        }

        public string ParticipantSearchQuery
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ParticipantSearchQuery));
                    _ = RefreshParticipantSearchResultsAsync();
                }
            }
        }

        public int DisplayedRound
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(DisplayedRound));
                }
            }
        }

        public ObservableCollection<Player> ParticipantSearchResults { get; } = new ObservableCollection<Player>();
        public ObservableCollection<CompetitionPageParticipantTableItem> ParticipantTableRows { get; } = new ObservableCollection<CompetitionPageParticipantTableItem>();
        public ObservableCollection<CompetitionPagePairingTableItem> PairingTableRows { get; } = new ObservableCollection<CompetitionPagePairingTableItem>();
        public ObservableCollection<CompetitionPageStandingTableItem> StandingsTableRows { get; } = new ObservableCollection<CompetitionPageStandingTableItem>();
        public ObservableCollection<CompetitionType> CompetitionTypeOptions { get; } = new ObservableCollection<CompetitionType>(Enum.GetValues(typeof(CompetitionType)).Cast<CompetitionType>());
        public ObservableCollection<TimeFormat> TimeFormatOptions { get; } = new ObservableCollection<TimeFormat>(Enum.GetValues(typeof(TimeFormat)).Cast<TimeFormat>());
        private IReadOnlyList<string> _resultOptions { get; } = [string.Empty, CommonConstants.WhiteWinResultString, CommonConstants.BlackWinResultString, CommonConstants.DrawResultString];
        private IReadOnlyList<string> _knockoutResultOptions { get; } = [string.Empty, CommonConstants.WhiteWinResultString, CommonConstants.BlackWinResultString];
        public IReadOnlyList<string> ActiveResultOptions => Competition?.Type == CompetitionType.KnockoutTournament ? _knockoutResultOptions : _resultOptions;
        public IEnumerable<double> ByeValues => [CommonConstants.DefaultLossScoreValue, CommonConstants.DefaultDrawScoreValue, CommonConstants.DefaultWinScoreValue];

        public bool IsNumberOfRoundsVisible => Competition?.Type == CompetitionType.SwissSystemTournament || Competition?.Type == CompetitionType.ChessMatch;
        public bool IsByeValueVisible => Competition?.Type == CompetitionType.SwissSystemTournament || Competition?.Type == CompetitionType.RoundRobinTournament;
        public bool IsDoubleRoundRobinVisible => Competition?.Type == CompetitionType.RoundRobinTournament;
        public bool IsStartCompetitionVisible => Competition?.State == CompetitionState.Starting;
        public bool IsStartCompetitionEnabled => Competition?.State == CompetitionState.Starting && (Competition.Type == CompetitionType.ChessMatch
            ? Competition.CompetitionParticipants.Count == 2
            : Competition.CompetitionParticipants.Count >= 2);
        public bool IsUndoRoundVisible => Competition?.State == CompetitionState.Ongoing;
        public bool IsAdvanceRoundVisible => Competition.State == CompetitionState.Ongoing && Competition.CurrentRound > 0 && Competition.CurrentRound < Competition.NumberOfRounds;
        public bool IsCompleteCompetitionVisible => Competition.State == CompetitionState.Ongoing && Competition.CurrentRound > 0 && Competition.CurrentRound == Competition.NumberOfRounds;
        public bool IsCompleteCompetitionEnabled => Competition.State == CompetitionState.Ongoing && Competition.CurrentRound > 0 && Competition.CurrentRound == Competition.NumberOfRounds && IsResultAssignedForAllCurrentRoundPairings;
        public bool IsAdvanceRoundEnabled => Competition?.State == CompetitionState.Ongoing && Competition.CurrentRound > 0 && Competition.CurrentRound < Competition.NumberOfRounds && IsResultAssignedForAllCurrentRoundPairings;
        public bool IsResultAssignedForAllCurrentRoundPairings => Competition.GetRoundPairings(Competition.CurrentRound, includeByes: false).All(pairing => pairing.Result.HasValue);
        public bool IsDisplayingCurrentRound => Competition != null && Competition.State == CompetitionState.Ongoing && Competition.CurrentRound > 0 && DisplayedRound == Competition.CurrentRound;
        public bool HasCurrentRoundPairings => PairingTableRows.Count > 0;
        public string ByeDisplay => "BYE: " + string.Join(", ", Competition.GetRoundByeParticipants(DisplayedRound).Select(participant => FormatHelper.FormatCompetitionParticipantNameWithRating(_clubService.CurrentClub, participant, _playerService.GetPlayerByIdentifierAsync(participant.ParticipantIdentifierName).GetAwaiter().GetResult())));
        public bool IsParticipantEditEnabled => Competition?.State == CompetitionState.Starting;
        public bool IsCompetitionInfoEditEnabled => Competition?.State == CompetitionState.Starting;
        public bool IsParticipantSearchVisible => IsParticipantEditEnabled && !(Competition.Type == CompetitionType.ChessMatch && ParticipantTableRows.Count >= 2) && !string.IsNullOrWhiteSpace(ParticipantSearchQuery) && ParticipantSearchResults.Count > 0;

        /// <summary>
        /// Private Fields and Constants
        /// </summary>
        public const int TabIndexInformation = 0;
        public const int TabIndexRounds = 1;
        public const int TabIndexStandings = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="pageNavigationService">The page navigation service.</param>
        /// <param name="clubService">The club service.</param>
        /// <param name="competitionService">The competition service.</param>
        /// <param name="playerService">The player service.</param>
        /// <param name="fideApiService">The FIDE API service.</param>
        /// <param name="gameService">The game service.</param>
        /// <param name="fileSystemService">The file system service.</param>
        /// <param name="messageBoxService">The message box service.</param>
        /// <param name="timeProvider">The time provider.</param>
        public CompetitionViewModel(
            ILogger logger,
            IPageNavigationService pageNavigationService,
            IClubService clubService,
            ICompetitionService competitionService,
            IPlayerService playerService,
            IFideApiService fideApiService,
            IGameService gameService,
            IFileSystemService fileSystemService,
            IMessageBoxService messageBoxService,
            TimeProvider timeProvider)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _clubService = clubService;
            _competitionService = competitionService;
            _playerService = playerService;
            _fideApiService = fideApiService;
            _gameService = gameService;
            _fileSystemService = fileSystemService;
            _messageBoxService = messageBoxService;
            _timeProvider = timeProvider;

            BackCommand = new RelayCommand(execute: _ => _pageNavigationService.NavigateBack());
            DeleteCommand = new RelayCommand(execute: async _ => await DeleteCompetitionAsync());
            ExportCommand = new RelayCommand(execute: async _ => await ExportCompetitionAsync());
            ExportToTrf16Command = new RelayCommand(execute: async _ => await ExportCompetitionToTrf16Async());
            AddParticipantCommand = new RelayCommand(execute: async parameter => await AddParticipantAsync(parameter), canExecute: _ => IsParticipantEditEnabled);
            RemoveParticipantCommand = new RelayCommand(execute: RemoveParticipant, canExecute: _ => IsParticipantEditEnabled);
            OpenParticipantPlayerCommand = new RelayCommand(
                execute: parameter =>
                {
                    if (parameter is CompetitionPageParticipantTableItem participantTableItem)
                    {
                        NavigateToPlayer(participantTableItem.IdentifierName);
                    }
                },
                canExecute: parameter => parameter is CompetitionPageParticipantTableItem participantTableItem && participantTableItem.CanNavigateToPlayer);
            UndoRoundCommand = new RelayCommand(execute: async _ => await UndoRoundAsync());
            StartCompetitionCommand = new RelayCommand(execute: async _ => await StartCompetitionAsync(), canExecute: _ => IsStartCompetitionEnabled);
            AdvanceRoundCommand = new RelayCommand(execute: async _ => await AdvanceRoundAsync());
            CompleteCompetitionCommand = new RelayCommand(execute: async _ => await CompleteCompetitionAsync());
            OpenPairingGameCommand = new RelayCommand(
                execute: parameter =>
                {
                    if (parameter is CompetitionPagePairingTableItem pairingTableItem)
                    {
                        NavigateToGame(pairingTableItem.GameId);
                    }
                },
                canExecute: parameter => parameter is CompetitionPagePairingTableItem pairingTableItem && pairingTableItem.CanNavigateToGame);
            PreviousRoundCommand = new RelayCommand(execute: _ =>
                {
                    DisplayedRound = Math.Max(1, DisplayedRound - 1);
                    RefreshRoundsTabElements();
                },
                canExecute: _ => Competition != null && Competition.State != CompetitionState.Starting && DisplayedRound > 1);
            NextRoundCommand = new RelayCommand(execute: _ =>
                {
                    DisplayedRound = Math.Min(Competition.NumberOfRounds, DisplayedRound + 1);
                    RefreshRoundsTabElements();
                },
                canExecute: _ => Competition != null && Competition.State != CompetitionState.Starting && DisplayedRound < Competition.NumberOfRounds);
        }

        /// <summary>
        /// Sets up the page after transition from another page.
        /// </summary>
        public override async void PageTransitionSetup(IDictionary<string, object> parameters)
        {
            // Set the loaded Competition object
            if (parameters.TryGetValue(nameof(Competition.Id), out object? idObj) && idObj is int competitionId)
            {
                Competition = await _competitionService.GetCompetitionByIdAsync(competitionId);
            }
            else
            {
                _logger.LogWarning("No CompetitionId parameter provided for CompetitionViewModel");
                _pageNavigationService.HardNavigateTo<DashboardViewModel>();
            }


            _winner = _playerService.GetPlayerByIdentifierAsync(Competition?.WinnerId).GetAwaiter().GetResult();
            DisplayedRound = Math.Max(1, Competition?.CurrentRound ?? 0);

            // Set selected tab
            if (parameters.TryGetValue(nameof(SelectedTabIndex), out object? idxObj) && idxObj is int tabIdx)
            {
                SelectedTabIndex = tabIdx;
            }
            else
            {
                SelectedTabIndex = TabIndexInformation;
            }

            RefreshSelectedTabElements();
            await RefreshParticipantSearchResultsAsync();
            RefreshPageActionElements();
            // Initialize displayed round for browsing pairings
            CommandManager.InvalidateRequerySuggested();
        }

        private void RefreshSelectedTabElements()
        {
            switch (SelectedTabIndex)
            {
                case TabIndexRounds:
                    RefreshRoundsTabElements();
                    break;
                case TabIndexStandings:
                    RefreshStandingsTabElements();
                    break;
                default:
                    RefreshInformationTabElements();
                    break;
            }
        }

        private void RefreshPageActionElements()
        {
            OnPropertyChanged(nameof(IsStartCompetitionVisible));
            OnPropertyChanged(nameof(IsStartCompetitionEnabled));
            OnPropertyChanged(nameof(IsUndoRoundVisible));
            OnPropertyChanged(nameof(IsAdvanceRoundVisible));
            OnPropertyChanged(nameof(IsAdvanceRoundEnabled));
            OnPropertyChanged(nameof(IsCompleteCompetitionVisible));
            OnPropertyChanged(nameof(IsCompleteCompetitionEnabled));
            CommandManager.InvalidateRequerySuggested();
        }

        private void RefreshInformationTabElements()
        {
            OnPropertyChanged(nameof(CompetitionTitle));
            OnPropertyChanged(nameof(CompetitionDescription));
            OnPropertyChanged(nameof(CompetitionCity));
            OnPropertyChanged(nameof(CompetitionChiefArbiter));
            OnPropertyChanged(nameof(CompetitionFederation));
            OnPropertyChanged(nameof(IsCompetitionRated));
            OnPropertyChanged(nameof(CompetitionNumberOfRounds));
            OnPropertyChanged(nameof(IsCompetitionDoubleRoundRobin));
            OnPropertyChanged(nameof(CompetitionByeValue));
            OnPropertyChanged(nameof(SelectedCompetitionType));
            OnPropertyChanged(nameof(SelectedTimeFormat));
            OnPropertyChanged(nameof(CompetitionStateDisplay));
            OnPropertyChanged(nameof(CompetitionStateBrushKey));
            OnPropertyChanged(nameof(WinnerDisplayName));
            OnPropertyChanged(nameof(ParticipantsLabel));
            OnPropertyChanged(nameof(IsNumberOfRoundsVisible));
            OnPropertyChanged(nameof(IsByeValueVisible));
            OnPropertyChanged(nameof(IsDoubleRoundRobinVisible));
            OnPropertyChanged(nameof(IsParticipantEditEnabled));
            OnPropertyChanged(nameof(IsCompetitionInfoEditEnabled));
            OnPropertyChanged(nameof(IsParticipantSearchVisible));
            _ = RefreshParticipantSearchResultsAsync();
            RefreshParticipantsTable();
            RefreshPageActionElements();
        }

        private string GetDisplayCompetitionTitle()
        {
            string? title = Competition?.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            string trimmedTitle = title.Trim();
            return trimmedTitle.Length > AppConstants.CompetitionPageTitleDisplayMaxLength
                ? trimmedTitle[..AppConstants.CompetitionPageTitleDisplayMaxLength] + "..."
                : trimmedTitle;
        }

        private bool TryValidateCompetitionTitle(string? title, out string validatedTitle)
        {
            validatedTitle = title?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(validatedTitle))
            {
                return true;
            }

            _messageBoxService.ShowError(LocalizationHelper.GetString("CompetitionTitleRequiredMessage"), LocalizationHelper.GetString("ValidationError"));
            return false;
        }

        private void RefreshRoundsTabElements()
        {
            OnPropertyChanged(nameof(DisplayedRound));
            OnPropertyChanged(nameof(CurrentRoundLabel));
            OnPropertyChanged(nameof(CurrentRoundLabelDisplay));
            OnPropertyChanged(nameof(IsDisplayingCurrentRound));
            OnPropertyChanged(nameof(ActiveResultOptions));
            OnPropertyChanged(nameof(ByeDisplay));
            OnPropertyChanged(nameof(RoundIndicator));
            RefreshPairingsTable();
        }

        private void RefreshStandingsTabElements()
        {
            RefreshStandingsTable();
        }

        private void NavigateToPlayer(string playerIdentifier)
        {
            _pageNavigationService.NavigateTo<PlayerViewModel>(
                pageParameters: new Dictionary<string, object>
                {
                    { nameof(Player.IdentifierName), playerIdentifier }
                },
                backParameters: new Dictionary<string, object>
                {
                    { nameof(Competition.Id), this.Competition!.Id },
                    { nameof(CompetitionViewModel.SelectedTabIndex), this.SelectedTabIndex }
                });
        }

        /// <summary>
        /// Load a new competition to the page.
        /// </summary>
        private void RefreshParticipantsTable()
        {
            ParticipantTableRows.Clear();

            foreach (CompetitionParticipant cp in Competition.CompetitionParticipants)
            {
                ParticipantTableRows.Add(FormatHelper.FormatCompetitionPageParticipantTableItem(_clubService.CurrentClub, cp, _playerService.GetPlayerByIdentifierAsync(cp.ParticipantIdentifierName).GetAwaiter().GetResult()));
            }

            OnPropertyChanged(nameof(ParticipantTableRows));
            OnPropertyChanged(nameof(ParticipantsLabel)); //Displays the participant count
        }

        private void RefreshPairingsTable()
        {
            // Add pairing table rows
            PairingTableRows.Clear();
            foreach (CompetitionPairing pairing in Competition.GetRoundPairings(round: DisplayedRound, includeByes: false).OrderBy(cp => cp.TableNumber))
            {
                CompetitionPagePairingTableItem pairingTableItem = new CompetitionPagePairingTableItem(
                    competitionPairing: pairing,
                    whitePlayer: _playerService.GetPlayerByIdentifierAsync(pairing.WhitePlayerIdentifierName).GetAwaiter().GetResult(),
                    blackPlayer: _playerService.GetPlayerByIdentifierAsync(pairing.BlackPlayerIdentifierName).GetAwaiter().GetResult(),
                    clubPreferences: _clubService.CurrentClub,
                    canNavigateToGame: CanNavigateToPairingGame(pairing));

                pairingTableItem.OnSetResult += UpdatePairingResultAsync;
                PairingTableRows.Add(pairingTableItem);
            }

            OnPropertyChanged(nameof(ActiveResultOptions));
            OnPropertyChanged(nameof(HasCurrentRoundPairings));
            OnPropertyChanged(nameof(CurrentRoundLabel));
            OnPropertyChanged(nameof(ByeDisplay));
            OnPropertyChanged(nameof(IsAdvanceRoundEnabled));
            OnPropertyChanged(nameof(IsCompleteCompetitionEnabled));
        }

        private void RefreshStandingsTable()
        {
            StandingsTableRows.Clear();

            foreach (CompetitionStandingEntry s in Competition.ComputedStandings)
            {
                StandingsTableRows.Add(FormatHelper.FormatCompetitionPageStandingTableItem(_clubService.CurrentClub, s, _playerService.GetPlayerByIdentifierAsync(s.Participant.ParticipantIdentifierName).GetAwaiter().GetResult()));
            }

            OnPropertyChanged(nameof(StandingsTableRows));
        }

        private async Task RefreshParticipantSearchResultsAsync()
        {
            ParticipantSearchResults.Clear();

            if (!IsParticipantEditEnabled || string.IsNullOrWhiteSpace(ParticipantSearchQuery))
            {
                OnPropertyChanged(nameof(IsParticipantSearchVisible));
                return;
            }

            string searchQuery = ParticipantSearchQuery;
            HashSet<string> existingParticipantIdentifiers = ParticipantTableRows
                .Select(participant => participant.IdentifierName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<Player> results = await _playerService.SearchPlayersAsync(searchQuery, null, 10);

            if (!string.Equals(searchQuery, ParticipantSearchQuery, StringComparison.Ordinal))
            {
                return;
            }

            foreach (Player player in results)
            {
                if (!existingParticipantIdentifiers.Contains(player.IdentifierName))
                {
                    ParticipantSearchResults.Add(player);
                }
            }

            OnPropertyChanged(nameof(IsParticipantSearchVisible));
        }

        /// <summary>
        /// Updates the result for a given competition pairing and persists the change.
        /// </summary>
        /// <param name="pairing">The competition pairing to update.</param>
        /// <param name="result">The new game result.</param>
        /// <returns>A task representing the async operation.</returns>
        public void UpdatePairingResultAsync(CompetitionPairing pairing, GameResult? result)
        {
            if (pairing == null || Competition == null)
                return;

            pairing.Result = result;
            Competition = _competitionService.UpdateCompetitionAsync(Competition).GetAwaiter().GetResult();
            RefreshPageActionElements();
            RefreshRoundsTabElements();
        }

        /// <summary>
        /// Add Participant button logic.
        /// </summary>
        private async Task AddParticipantAsync(object? parameter)
        {
            if (parameter is not Player player)
            {
                return;
            }
            if (ParticipantTableRows.Any(p => string.Equals(p.IdentifierName, player.IdentifierName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            Competition = await _competitionService.AddParticipantAsync(Competition, player.IdentifierName);

            RefreshInformationTabElements();

            // Clear the search box and hide suggestions until the user types again
            ParticipantSearchResults.Clear();
            ParticipantSearchQuery = string.Empty;
            OnPropertyChanged(nameof(IsParticipantSearchVisible));
        }

        /// <summary>
        /// Remove Participant button logic.
        /// </summary>
        private async void RemoveParticipant(object? parameter)
        {
            if (!IsParticipantEditEnabled || parameter is not CompetitionPageParticipantTableItem participantRow)
            {
                return;
            }

            Competition = await _competitionService.RemoveParticipantAsync(Competition, participantRow.IdentifierName);

            RefreshInformationTabElements();
        }

        /// <summary>
        /// Export button logic.
        /// </summary>
        private async Task ExportCompetitionAsync()
        {
            if (Competition == null)
            {
                _messageBoxService.ShowError(LocalizationHelper.GetString("NoCompetitionSelectedMessage"), LocalizationHelper.GetString("ErrorTitle"));
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = Competition.Title,
                DefaultExt = ".xlsx",
                Filter = LocalizationHelper.GetString("ExportFileDialogFilter")
            };

            bool? result = dlg.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dlg.FileName))
                return;

            try
            {
                IEnumerable<CompetitionPageStandingTableItem> standingsToExport;
                if (StandingsTableRows.Any())
                {
                    standingsToExport = StandingsTableRows.ToList();
                }
                else
                {
                    var computedStandings = Competition.ComputedStandings ?? StandingsHelper.ComputeStandingEntries(Competition);
                    standingsToExport = computedStandings
                        .Select(s => FormatHelper.FormatCompetitionPageStandingTableItem(_clubService.CurrentClub, s, _playerService.GetPlayerByIdentifierAsync(s.Participant.ParticipantIdentifierName).GetAwaiter().GetResult()))
                        .ToList();
                }

                await Task.Run(() => ExportToExcelHelper.ExportCompetitionToExcel(Competition, dlg.FileName, standingsToExport, _playerService, _timeProvider));
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("ExportSuccessMessage"), LocalizationHelper.GetString("ExportCompleteTitle"));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError(LocalizationHelper.GetString("ExportFailedMessage") + "\n" + ex.Message, LocalizationHelper.GetString("ErrorTitle"));
            }
        }

        /// <summary>
        /// Exports a completed competition to a user-selected TRF16 file.
        /// </summary>
        private async Task ExportCompetitionToTrf16Async()
        {
            if (Competition == null)
            {
                _messageBoxService.ShowError(LocalizationHelper.GetString("NoCompetitionSelectedMessage"), LocalizationHelper.GetString("ErrorTitle"));
                return;
            }

            if (Competition.State != CompetitionState.Completed)
            {
                _messageBoxService.ShowWarning(
                    LocalizationHelper.GetString("TrfExportCompletedCompetitionOnlyMessage"),
                    ExportButtonLabel);
                return;
            }

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Competition.Title,
                DefaultExt = ".trf",
                Filter = LocalizationHelper.GetString("TrfExportFileDialogFilter"),
                AddExtension = true,
            };

            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return;
            }

            try
            {
                string trfContent = await ExportToTRFHelper.ExportCompetitionToTrf16(
                    Competition,
                    _clubService.CurrentClub,
                    _playerService,
                    _fideApiService);
                _fileSystemService.WriteAllText(dialog.FileName, trfContent);
                _messageBoxService.ShowInfo(LocalizationHelper.GetString("ExportSuccessMessage"), LocalizationHelper.GetString("ExportCompleteTitle"));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError(LocalizationHelper.GetString("ExportFailedMessage") + "\n" + ex.Message, LocalizationHelper.GetString("ErrorTitle"));
            }
        }

        /// <summary>
        /// Delete button logic.
        /// </summary>
        private async Task DeleteCompetitionAsync()
        {
            if (Competition == null)
                return;

            CustomMessageBoxResult result = _messageBoxService.ShowConfirmation("Delete competition?", "Confirm");

            if (result == CustomMessageBoxResult.Yes)
            {
                await _competitionService.DeleteCompetitionAsync(Competition.Id);
                _pageNavigationService.NavigateTo<CompetitionsViewModel>();
            }
        }

        /// <summary>
        /// Start Competition button logic.
        /// </summary>
        private async Task StartCompetitionAsync()
        {
            if (Competition == null)
                return;

            if (Competition.Type == CompetitionType.SwissSystemTournament)
            {
                int maxNumberOfRounds = SwissPairingAlgorithm.CalculateMaxNumberOfRounds(ParticipantTableRows.Count);

                if (Competition.NumberOfRounds > maxNumberOfRounds)
                {
                    CustomMessageBoxResult result = _messageBoxService.Show(
                        string.Format(LocalizationHelper.GetString("SwissCompetitionMaxRoundsExceededMessage"), maxNumberOfRounds),
                        LocalizationHelper.GetString("Confirm"),
                        CustomMessageBoxButton.OKCancel,
                        CustomMessageBoxImage.Warning);

                    if (result != CustomMessageBoxResult.OK)
                    {
                        SelectedTabIndex = TabIndexInformation;
                        return;
                    }
                }
            }

            Competition = await _competitionService.StartCompetitionAsync(Competition);
            DisplayedRound = Competition.CurrentRound;
            RefreshInformationTabElements();
            RefreshPageActionElements();

            // Switch to Rounds tab
            SelectedTabIndex = TabIndexRounds;
        }

        /// <summary>
        /// Undo Round button logic.
        /// </summary>
        private async Task UndoRoundAsync()
        {
            if (Competition == null)
                return;

            CustomMessageBoxResult confirm = _messageBoxService.ShowConfirmation(LocalizationHelper.GetString("UndoRoundConfirmation"), LocalizationHelper.GetString("Confirm"));

            if (confirm == CustomMessageBoxResult.Yes)
            {
                Competition = await _competitionService.UndoRoundAsync(Competition);
                DisplayedRound = Competition.CurrentRound;
                RefreshInformationTabElements();
                RefreshPageActionElements();
            }

            // Switch to Rounds or Information tab depending on resulting competition state
            SelectedTabIndex = Competition.State == CompetitionState.Ongoing ? CompetitionViewModel.TabIndexRounds : CompetitionViewModel.TabIndexInformation;
        }

        /// <summary>
        /// Advance Round button logic.
        /// </summary>
        private async Task AdvanceRoundAsync()
        {
            if (Competition == null)
                return;

            Competition = await _competitionService.AdvanceRoundAsync(Competition);
            DisplayedRound = Competition.CurrentRound;
            RefreshPageActionElements();
            RefreshRoundsTabElements();

            // Switch to Rounds tab
            SelectedTabIndex = CompetitionViewModel.TabIndexRounds;
        }

        /// <summary>
        /// Complete Competition button logic.
        /// </summary>
        private async Task CompleteCompetitionAsync()
        {
            if (Competition == null)
                return;

            CustomMessageBoxResult confirm = _messageBoxService.ShowConfirmation(LocalizationHelper.GetString("CompleteCompetitionConfirmation"), LocalizationHelper.GetString("Confirm"));

            if (confirm == CustomMessageBoxResult.Yes)
            {
                Competition = await _competitionService.CompleteCompetitionAsync(Competition);
                _winner = _playerService.GetPlayerByIdentifierAsync(Competition?.WinnerId).GetAwaiter().GetResult();
                RefreshInformationTabElements();
                RefreshPageActionElements();

                // Switch to Standings tab after completion
                SelectedTabIndex = CompetitionViewModel.TabIndexStandings;
            }
        }

        public bool CanNavigateToPairingGame(CompetitionPairing pairing)
        {
            if (Competition == null || pairing == null || !pairing.GameId.HasValue || !_gameService.GameExists(pairing.GameId.Value) || Competition.State != CompetitionState.Completed)
            {
                return false;
            }

            return true;
        }

        public void NavigateToGame(int? gameId)
        {
            if (!gameId.HasValue)
            {
                return;
            }

            // Setup back parameters so going back from Game page still displays the same selected tab
            _pageNavigationService.NavigateTo<GameViewModel>(
                pageParameters: new Dictionary<string, object>
                {
                    { nameof(Game.Id), gameId.Value }
                },
                backParameters: new Dictionary<string, object>
                {
                    { nameof(Competition.Id), this.Competition!.Id },
                    { nameof(CompetitionViewModel.SelectedTabIndex), this.SelectedTabIndex }
                }
            );
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            base.OnLanguageChanged(_);
            OnPropertyChanged(nameof(CompetitionPage));
            OnPropertyChanged(nameof(InformationTabHeader));
            OnPropertyChanged(nameof(RoundsTabHeader));
            OnPropertyChanged(nameof(StandingsTabHeader));
            OnPropertyChanged(nameof(TitleLabel));
            OnPropertyChanged(nameof(DescriptionLabel));
            OnPropertyChanged(nameof(CityLabel));
            OnPropertyChanged(nameof(ChiefArbiterLabel));
            OnPropertyChanged(nameof(FederationLabel));
            OnPropertyChanged(nameof(DisplayNameLabel));
            OnPropertyChanged(nameof(IdentifierNameLabel));
            OnPropertyChanged(nameof(TypeLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(TimeFormatOptions));
            OnPropertyChanged(nameof(CompetitionTypeOptions));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(NumberOfRoundsLabel));
            OnPropertyChanged(nameof(DoubleRoundRobinLabel));
            OnPropertyChanged(nameof(ByeValueLabel));
            OnPropertyChanged(nameof(WinnerLabel));
            OnPropertyChanged(nameof(SaveButtonLabel));
            OnPropertyChanged(nameof(DeleteButtonLabel));
            OnPropertyChanged(nameof(BackButtonLabel));
            OnPropertyChanged(nameof(StartCompetitionButtonLabel));
            OnPropertyChanged(nameof(AdvanceRoundButtonLabel));
            OnPropertyChanged(nameof(CompleteCompetitionButtonLabel));
            OnPropertyChanged(nameof(UndoRoundButtonLabel));
            OnPropertyChanged(nameof(ExportButtonLabel));
            OnPropertyChanged(nameof(ExportToExcelLabel));
            OnPropertyChanged(nameof(ExportToTrf16Label));
            OnPropertyChanged(nameof(CurrentRoundLabelDisplay));
            OnPropertyChanged(nameof(ParticipantsLabel));
            OnPropertyChanged(nameof(SearchPlaceholder));
            OnPropertyChanged(nameof(AddParticipantButtonLabel));
            OnPropertyChanged(nameof(RemoveParticipantButtonLabel));
            OnPropertyChanged(nameof(RoundsPlaceholder));
            OnPropertyChanged(nameof(PreviousPageButtonLabel));
            OnPropertyChanged(nameof(NextPageButtonLabel));
            OnPropertyChanged(nameof(CurrentRoundLabel));
            OnPropertyChanged(nameof(TableNumberLabel));
            OnPropertyChanged(nameof(WhitePlayerLabel));
            OnPropertyChanged(nameof(ResultLabel));
            OnPropertyChanged(nameof(BlackPlayerLabel));
            OnPropertyChanged(nameof(ClassificationLabel));
            OnPropertyChanged(nameof(ParticipantLabel));
            OnPropertyChanged(nameof(ScoreLabel));
            OnPropertyChanged(nameof(OldRatingLabel));
            OnPropertyChanged(nameof(NewRatingLabel));
            OnPropertyChanged(nameof(PerformanceRatingLabel));
            OnPropertyChanged(nameof(BuchholzScoreLabel));
            OnPropertyChanged(nameof(SonnebornBergerScoreLabel));
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(CompetitionTitleHelp));
            OnPropertyChanged(nameof(CompetitionDescriptionHelp));
            OnPropertyChanged(nameof(CompetitionCityHelp));
            OnPropertyChanged(nameof(CompetitionChiefArbiterHelp));
            OnPropertyChanged(nameof(CompetitionFederationHelp));
            OnPropertyChanged(nameof(CompetitionWinnerHelp));
            OnPropertyChanged(nameof(CompetitionTimeFormatHelp));
            OnPropertyChanged(nameof(CompetitionRatedHelp));
            OnPropertyChanged(nameof(CompetitionTypeHelp));
            OnPropertyChanged(nameof(CompetitionRoundsHelp));
            OnPropertyChanged(nameof(CompetitionDoubleRoundRobinHelp));
            OnPropertyChanged(nameof(CompetitionByeValueHelp));
            OnPropertyChanged(nameof(CompetitionParticipantSearchHelp));
            OnPropertyChanged(nameof(EditCompetitionTitleTooltip));
            OnPropertyChanged(nameof(EditCompetitionDescriptionTooltip));
            OnPropertyChanged(nameof(EditCompetitionCityTooltip));
            OnPropertyChanged(nameof(EditCompetitionChiefArbiterTooltip));
            OnPropertyChanged(nameof(EditCompetitionFederationTooltip));
            OnPropertyChanged(nameof(CompetitionWinnerTooltip));
            OnPropertyChanged(nameof(EditCompetitionTimeFormatTooltip));
            OnPropertyChanged(nameof(EditCompetitionRatedTooltip));
            OnPropertyChanged(nameof(EditCompetitionTypeTooltip));
            OnPropertyChanged(nameof(EditCompetitionRoundsTooltip));
            OnPropertyChanged(nameof(EditCompetitionDoubleRoundRobinTooltip));
            OnPropertyChanged(nameof(EditCompetitionByeValueTooltip));
            OnPropertyChanged(nameof(CompetitionParticipantSearchTooltip));
            RefreshPageActionElements();
            RefreshInformationTabElements();
            RefreshRoundsTabElements();
            RefreshStandingsTabElements();
        }
    }
}
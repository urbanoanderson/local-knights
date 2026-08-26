using System.Collections.ObjectModel;
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
    /// ViewModel for the Create Competition page.
    /// </summary>
    public class CreateCompetitionViewModel : BaseAppViewModel
    {
        private readonly ILogger _logger;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly ICompetitionService _competitionService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCompetitionViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording service events.</param>
        /// <param name="pageNavigationService">The page navigation service.</param>
        /// <param name="competitionService">The competition service.</param>
        /// <param name="messageBoxService">The message box service.</param>
        /// <param name="timeProvider">The time provider for handling time-related operations.</param>
        public CreateCompetitionViewModel(ILogger logger,
            IPageNavigationService pageNavigationService,
            ICompetitionService competitionService,
            IMessageBoxService messageBoxService,
            TimeProvider timeProvider)
        {
            _logger = logger;
            _pageNavigationService = pageNavigationService;
            _competitionService = competitionService;
            _messageBoxService = messageBoxService;
            _timeProvider = timeProvider;

            CreateCommand = new RelayCommand(async _ => await CreateCompetitionAsync());
            CancelCommand = new RelayCommand(_ => _pageNavigationService.NavigateBack());

            LoadInitialOptions();
        }

        private string _title = string.Empty;
        /// <summary>
        /// Gets or sets the competition title.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _description = string.Empty;
        /// <summary>
        /// Gets or sets the competition description.
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

        private CompetitionType _type;

        /// <summary>
        /// Gets or sets the competition type.
        /// </summary>
        public CompetitionType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(IsNumberOfRoundsVisible));
                OnPropertyChanged(nameof(IsByeValueVisible));
                OnPropertyChanged(nameof(IsDoubleRoundRobinVisible));
            }
        }

        /// <summary>
        private TimeFormat _timeFormat = CommonConstants.DefaultTimeFormat;

        /// <summary>
        /// Gets or sets the time format.
        /// </summary>
        public TimeFormat TimeFormat
        {
            get => _timeFormat;
            set
            {
                _timeFormat = value;
                OnPropertyChanged(nameof(TimeFormat));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the competition is rated.
        /// </summary>
        public bool IsRated { get; set; } = CommonConstants.DefaultCompetitionIsRatedValue;

        private int _numberOfRounds = CommonConstants.DefaultCompetitionNumberOfRounds;
        /// <summary>
        /// Gets or sets the number of rounds.
        /// </summary>
        public int NumberOfRounds
        {
            get => _numberOfRounds;
            set
            {
                _numberOfRounds = value;
                OnPropertyChanged(nameof(NumberOfRounds));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it's double round-robin.
        /// </summary>
        public bool IsDoubleRoundRobin { get; set; }

        private double _byeValue = CommonConstants.DefaultByeValue;
        /// <summary>
        /// Gets or sets the bye value.
        /// </summary>
        public double ByeValue
        {
            get => _byeValue;
            set
            {
                _byeValue = value;
                OnPropertyChanged(nameof(ByeValue));
            }
        }

        /// <summary>
        /// Gets the available bye value options.
        /// </summary>
        public IEnumerable<double> ByeValues => new[] { CommonConstants.DefaultLossScoreValue, CommonConstants.DefaultDrawScoreValue, CommonConstants.DefaultWinScoreValue };

        /// <summary>
        /// Gets a value indicating whether the number of rounds field should be displayed.
        /// </summary>
        public bool IsNumberOfRoundsVisible => Type == CompetitionType.SwissSystemTournament || Type == CompetitionType.ChessMatch;

        /// <summary>
        /// Gets a value indicating whether the bye value field should be displayed.
        /// </summary>
        public bool IsByeValueVisible => Type == CompetitionType.SwissSystemTournament || Type == CompetitionType.RoundRobinTournament;

        /// <summary>
        /// Gets a value indicating whether the double round-robin option should be displayed.
        /// </summary>
        public bool IsDoubleRoundRobinVisible => Type == CompetitionType.RoundRobinTournament;

        /// <summary>
        /// Gets the command to create the competition.
        /// </summary>
        public ICommand CreateCommand { get; }

        /// <summary>
        /// <summary>
        /// Time format options for the UI.
        /// </summary>
        public ObservableCollection<TimeFormat> TimeFormatOptions { get; } = new ObservableCollection<TimeFormat>();

        /// <summary>
        /// Competition type options for the UI.
        /// </summary>
        public ObservableCollection<CompetitionType> CompetitionTypeOptions { get; } = new ObservableCollection<CompetitionType>();

        /// <summary>
        /// Gets the command to cancel.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets the localized page title for the competition creation screen.
        /// </summary>
        public string CreateCompetition => $"{AppConstants.DefaultCompetitionIcon} " + LocalizationHelper.GetString("CreateCompetition");

        /// <summary>
        /// Gets the localized label for the competition title input.
        /// </summary>
        public string TitleLabel => LocalizationHelper.GetString("TitleLabel");

        /// <summary>
        /// Gets the localized label for the competition description input.
        /// </summary>
        public string DescriptionLabel => LocalizationHelper.GetString("DescriptionLabel");

        /// <summary>
        /// Gets the localized label for the competition type selection.
        /// </summary>
        public string TypeLabel => LocalizationHelper.GetString("TypeLabel");

        /// <summary>
        /// Gets the localized label for the time format selection.
        /// </summary>
        public string TimeFormatLabel => LocalizationHelper.GetString("TimeFormatLabel");

        /// <summary>
        /// Gets the localized label for the rated checkbox.
        /// </summary>
        public string RatedLabel => LocalizationHelper.GetString("RatedLabel");

        /// <summary>
        /// Gets the localized label for the number of rounds input.
        /// </summary>
        public string NumberOfRoundsLabel => LocalizationHelper.GetString("NumberOfRoundsLabel");

        /// <summary>
        /// Gets the localized label for the double round-robin checkbox.
        /// </summary>
        public string DoubleRoundRobinLabel => LocalizationHelper.GetString("DoubleRoundRobinLabel");

        /// <summary>
        /// Gets the localized label for the bye value input.
        /// </summary>
        public string ByeValueLabel => LocalizationHelper.GetString("ByeValueLabel");

        /// <summary>
        /// Gets the localized label for the Create button.
        /// </summary>
        public string CreateButtonLabel => LocalizationHelper.GetString("CreateButtonLabel");

        /// <summary>
        /// Gets the localized label for the Cancel button.
        /// </summary>
        public string CancelLabel => LocalizationHelper.GetString("CancelLabel");

        private async Task CreateCompetitionAsync()
        {
            if (!TryValidateCompetitionTitle(Title, out string validatedTitle))
            {
                return;
            }

            Competition competition = new Competition
            {
                CreatedAt = _timeProvider.GetUtcNow().DateTime,
                Title = validatedTitle,
                Description = Description,
                Type = Type,
                TimeFormat = TimeFormat,
                IsRated = IsRated,
                NumberOfRounds = NumberOfRounds,
                IsDoubleRoundRobin = IsDoubleRoundRobin,
                ByeValue = ByeValue
            };

            await _competitionService.CreateCompetitionAsync(competition);
            _pageNavigationService.HardNavigateTo<CompetitionsViewModel>();
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

        private void LoadInitialOptions()
        {
            TimeFormat selectedTimeFormat = TimeFormat;
            CompetitionType selectedCompetitionType = Type;

            TimeFormatOptions.Clear();
            foreach (TimeFormat timeFormat in Enum.GetValues(typeof(TimeFormat)).Cast<TimeFormat>())
            {
                TimeFormatOptions.Add(timeFormat);
            }

            CompetitionTypeOptions.Clear();
            foreach (CompetitionType t in Enum.GetValues(typeof(CompetitionType)).Cast<CompetitionType>())
            {
                CompetitionTypeOptions.Add(t);
            }

            TimeFormat = selectedTimeFormat;
            Type = selectedCompetitionType;

            // Ensure selected values are in sync
            OnPropertyChanged(nameof(TimeFormatOptions));
            OnPropertyChanged(nameof(CompetitionTypeOptions));
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public override void OnLanguageChanged(string _)
        {
            OnPropertyChanged(nameof(CreateCompetition));
            OnPropertyChanged(nameof(TitleLabel));
            OnPropertyChanged(nameof(DescriptionLabel));
            OnPropertyChanged(nameof(TimeFormatLabel));
            OnPropertyChanged(nameof(RatedLabel));
            OnPropertyChanged(nameof(TypeLabel));
            OnPropertyChanged(nameof(NumberOfRoundsLabel));
            OnPropertyChanged(nameof(DoubleRoundRobinLabel));
            OnPropertyChanged(nameof(ByeValueLabel));
            OnPropertyChanged(nameof(CreateButtonLabel));
            OnPropertyChanged(nameof(CancelLabel));
            LoadInitialOptions();
        }
    }
}
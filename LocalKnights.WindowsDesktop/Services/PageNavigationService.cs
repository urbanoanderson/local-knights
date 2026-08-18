using Microsoft.Extensions.Logging;
using LocalKnights.WindowsDesktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using LocalKnights.Common.Services.Database;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Defines an interface for controling page navigation in the application.
    /// </summary>
    public sealed class PageNavigationService : IPageNavigationService
    {
        private readonly ILogger _logger;
        private readonly IClubService _clubService;
        private readonly IUISettingsService _uiSettingsService;
        private readonly Stack<PageNavigationEntry> _pageStack = new();

        private sealed record PageNavigationEntry(BaseAppViewModel Page, IDictionary<string, object> PageParameters);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemService"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording service events.</param>
        /// <param name="clubService">The club service for checking if is club signed in.</param>
        /// <param name="uiSettingsService">The UI settings service for managing user interface settings changes.</param>
        public PageNavigationService(ILogger logger, IClubService clubService, IUISettingsService uiSettingsService)
        {
            _logger = logger;
            _clubService = clubService;
            _uiSettingsService = uiSettingsService;
        }

        /// <inheritdoc/>
        public event Action<BaseAppViewModel>? OnSetActivePage;

        /// <summary>
        /// Gets the current ViewModel being displayed in the main content area.
        /// </summary>
        public BaseAppViewModel ActivePage => _pageStack.Peek().Page;

        /// <inheritdoc/>
        public void NavigateTo<TPage>(IDictionary<string, object> pageParameters, IDictionary<string, object> backParameters) where TPage : BaseAppViewModel
        {
            PushPage<TPage>(pageParameters, backParameters);
        }

        /// <inheritdoc/>
        public void NavigateTo<TPage>(IDictionary<string, object> pageParameters) where TPage : BaseAppViewModel
        {
            this.NavigateTo<TPage>(pageParameters, new Dictionary<string, object>());
        }

        /// <inheritdoc/>
        public void NavigateTo<TPage>() where TPage : BaseAppViewModel
        {
            this.NavigateTo<TPage>(new Dictionary<string, object>(), new Dictionary<string, object>());
        }

        /// <inheritdoc/>
        public void HardNavigateTo<TPage>(IDictionary<string, object> pageParameters, IDictionary<string, object> backParameters) where TPage : BaseAppViewModel
        {
            _pageStack.Clear();
            PushPage<TPage>(pageParameters, backParameters);
        }

        /// <inheritdoc/>
        public void HardNavigateTo<TPage>(IDictionary<string, object> pageParameters) where TPage : BaseAppViewModel
        {
            HardNavigateTo<TPage>(pageParameters, new Dictionary<string, object>());
        }

        /// <inheritdoc/>
        public void HardNavigateTo<TPage>() where TPage : BaseAppViewModel
        {
            HardNavigateTo<TPage>(new Dictionary<string, object>(), new Dictionary<string, object>());
        }

        /// <inheritdoc/>
        public void NavigateBack()
        {
            if (_pageStack.Count > 0)
            {
                _pageStack.Pop();
            }

            if (_pageStack.Count == 0)
            {
                NavigateToDefaultPage();
                return;
            }

            PageNavigationEntry pageEntry = _pageStack.Peek();
            _uiSettingsService.OnSetLanguage += _pageStack.Peek().Page.OnLanguageChanged;
            _uiSettingsService.OnSetPageSize += _pageStack.Peek().Page.OnPageSizeChanged;
            pageEntry.Page.PageTransitionSetup(pageEntry.PageParameters);
            NotifyActivePageChanged();
        }

        private void PushPage<TPage>(IDictionary<string, object> pageParameters, IDictionary<string, object> backParameters) where TPage : BaseAppViewModel
        {
            //Apply back parameters to current page if provided
            if (backParameters != null && _pageStack.Count > 0)
            {
                foreach (var kv in backParameters)
                {
                    _pageStack.Peek().PageParameters[kv.Key] = kv.Value;
                }
            }

            // Disable previous page events
            if (_pageStack.TryPeek(out PageNavigationEntry top))
            {
                BaseAppViewModel prevPage = top.Page;
                _uiSettingsService.OnSetLanguage -= prevPage.OnLanguageChanged;
                _uiSettingsService.OnSetPageSize -= prevPage.OnPageSizeChanged;
            }

            // Instantiate and add new page to stack
            BaseAppViewModel nextPage = ServiceProviderManager.ServiceProvider.GetRequiredService<TPage>();
            _uiSettingsService.OnSetLanguage += nextPage.OnLanguageChanged;
            _uiSettingsService.OnSetPageSize += nextPage.OnPageSizeChanged;
            nextPage.PageTransitionSetup(pageParameters);
            _pageStack.Push(new PageNavigationEntry(nextPage, pageParameters));
            NotifyActivePageChanged();
        }

        private void NavigateToDefaultPage()
        {
            if (_clubService.CurrentClub != null)
            {
                HardNavigateTo<DashboardViewModel>();
                return;
            }

            HardNavigateTo<StartViewModel>();
        }

        private void NotifyActivePageChanged()
        {
            if (_pageStack.Count == 0)
            {
                _logger.LogWarning("No active page to notify.");
                return;
            }

            OnSetActivePage?.Invoke(ActivePage);
        }
    }
}

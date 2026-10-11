using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Defines an interface for controling page navigation in the application.
    /// </summary>
    public interface IPageNavigationService
    {
        /// <summary>
        /// The current active page.
        /// </summary>

        BaseAppViewModel ActivePage { get; }

        /// <summary>
        /// Event when setting a new active page.
        /// </summary>
        event Action<BaseAppViewModel>? OnSetActivePage;

        /// <summary>
        /// Navigates to a specified page with optional parameters. The page type is determined by the generic type parameter TPage, which must be a subclass of BaseAppViewModel. The pageParameters dictionary can be used to pass any necessary data to the new page during navigation and the backParameters allow for configure the page for when another page goes back.
        /// </summary>
        /// <param name="pageParameters">The parameters for the page to navigate to.</param>
        /// <param name="backParameters">The parameters for the page to navigate back to.</param>
        void NavigateTo<TPage>(IDictionary<string, object> pageParameters, IDictionary<string, object> backParameters) where TPage : BaseAppViewModel;

        /// <summary>
        /// Navigates to a specified page with optional parameters. The page type is determined by the generic type parameter TPage, which must be a subclass of BaseAppViewModel. The pageParameters dictionary can be used to pass any necessary data to the new page during navigation.
        /// </summary>
        /// <param name="pageParameters">The parameters for the page to navigate to.</param>
        void NavigateTo<TPage>(IDictionary<string, object> pageParameters) where TPage : BaseAppViewModel;

        /// <summary>
        /// Navigates to a specified page without parameters. The page type is determined by the generic type parameter TPage, which must be a subclass of BaseAppViewModel.
        /// </summary>
        void NavigateTo<TPage>() where TPage : BaseAppViewModel;

        /// <summary>
        /// Clears the navigation stack and navigates to the specified page with optional parameters and back parameters.
        /// </summary>
        /// <param name="pageParameters">The parameters for the page to navigate to.</param>
        /// <param name="backParameters">The parameters for the page to navigate back to.</param>
        void HardNavigateTo<TPage>(IDictionary<string, object> pageParameters, IDictionary<string, object> backParameters) where TPage : BaseAppViewModel;

        /// <summary>
        /// Clears the navigation stack and navigates to the specified page with optional parameters.
        /// </summary>
        /// <param name="pageParameters">The parameters for the page to navigate to.</param>
        void HardNavigateTo<TPage>(IDictionary<string, object> pageParameters) where TPage : BaseAppViewModel;

        /// <summary>
        /// Clears the navigation stack and navigates to the specified page without parameters.
        /// </summary>
        void HardNavigateTo<TPage>() where TPage : BaseAppViewModel;

        /// <summary>
        /// Removes the current page from the navigation stack and activates the previous page.
        /// When the stack becomes empty, navigates to the default page based on club sign-in state.
        /// </summary>
        void NavigateBack();
    }
}

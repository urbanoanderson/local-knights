using System.ComponentModel;

namespace LocalKnights.WindowsDesktop.ViewModels
{
    /// <summary>
    /// Base ViewModel class for app pages.
    /// </summary>
    public abstract class BaseAppViewModel : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify UI elements that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Optional method to utilize page parameters received during page transition.
        /// </summary>
        /// <param name="pageParameters">Page parameters dictionary to configure page.</param>
        public virtual void PageTransitionSetup(IDictionary<string, object> pageParameters)
        {
        }

        /// <summary>
        /// Handles property change notifications from language change in app settings to update localized text.
        /// </summary>
        public virtual void OnLanguageChanged(string cultureCode)
        {
        }

        /// <summary>
        /// Handles property change notifications from page size change in app settings to update displayed tables.
        /// </summary>
        public virtual void OnPageSizeChanged(int pageSize)
        {
        }
    }
}
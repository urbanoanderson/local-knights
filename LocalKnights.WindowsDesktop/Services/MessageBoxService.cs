using System.Windows;
using LocalKnights.WindowsDesktop.Views;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Provides message box interactions for the application.
    /// </summary>
    public class MessageBoxService : IMessageBoxService
    {
        /// <inheritdoc/>
        public CustomMessageBoxResult ShowInfo(string message, string title = "")
        {
            return Show(message, title, CustomMessageBoxButton.OK, CustomMessageBoxImage.Information);
        }

        /// <inheritdoc/>
        public CustomMessageBoxResult ShowWarning(string message, string title = "")
        {
            return Show(message, title, CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
        }

        /// <inheritdoc/>
        public CustomMessageBoxResult ShowError(string message, string title = "")
        {
            return Show(message, title, CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
        }

        /// <inheritdoc/>
        public CustomMessageBoxResult ShowConfirmation(string message, string title = "")
        {
            return Show(message, title, CustomMessageBoxButton.YesNo, CustomMessageBoxImage.Warning);
        }

        /// <inheritdoc/>
        public CustomMessageBoxResult Show(string message, string title, CustomMessageBoxButton button, CustomMessageBoxImage icon)
        {
            CustomMessageBoxWindow dialog = new(message, title, (MessageBoxButton)button, (MessageBoxImage)icon)
            {
                Owner = Application.Current?.MainWindow
            };

            dialog.ShowDialog();
            return (CustomMessageBoxResult)dialog.Result;
        }
    }
}

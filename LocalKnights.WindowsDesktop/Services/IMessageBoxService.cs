using System.Windows;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Defines an interface for displaying message boxes in the application. This allows for abstraction and easier testing of message box interactions.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Shows an informational message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The window title.</param>
        CustomMessageBoxResult ShowInfo(string message, string title = "");

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The window title.</param>
        CustomMessageBoxResult ShowWarning(string message, string title = "");

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The window title.</param>
        CustomMessageBoxResult ShowError(string message, string title = "");

        /// <summary>
        /// Shows a confirmation message box (Yes/No).
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The window title.</param>
        CustomMessageBoxResult ShowConfirmation(string message, string title = "");

        /// <summary>
        /// Shows a customizable message box with specified buttons and icon.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The window title.</param>
        /// <param name="button">The button configuration.</param>
        /// <param name="icon">The icon to display.</param>
        CustomMessageBoxResult Show(string message, string title, CustomMessageBoxButton button, CustomMessageBoxImage icon);
    }

    public enum CustomMessageBoxButton
    {
        OK,
        OKCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel,
        CancelTryContinue
    }

    public enum CustomMessageBoxImage
    {
        None = 0,
        Hand = 16,
        Question = 32,
        Exclamation = 48,
        Asterisk = 64,
        Stop = 16,
        Error = 16,
        Warning = 48,
        Information = 64
    }

    public enum CustomMessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
        TryAgain = 10,
        Continue = 11
    }
}

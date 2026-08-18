using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocalKnights.WindowsDesktop.Views
{
    /// <summary>
    /// Interaction logic for CustomMessageBoxWindow.xaml.
    /// </summary>
    public partial class CustomMessageBoxWindow : Window
    {
        private MessageBoxResult _closeResult = MessageBoxResult.None;

        /// <summary>
        /// Gets the result of the message box interaction.
        /// </summary>
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxWindow"/> class.
        /// </summary>
        /// <param name="message">The message text to display.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="buttons">The button configuration.</param>
        /// <param name="icon">The icon to display.</param>
        public CustomMessageBoxWindow(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();
            Title = title;
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            ConfigureIcon(icon);
            ConfigureButtons(buttons);
        }

        private void ConfigureIcon(MessageBoxImage icon)
        {
            string iconText;
            Brush iconBrush;

            switch (icon)
            {
                case MessageBoxImage.Warning:
                    iconText = "!";
                    iconBrush = (Brush)FindResource("WarningBrush");
                    break;
                case MessageBoxImage.Error:
                    iconText = "✖";
                    iconBrush = (Brush)FindResource("DangerBrush");
                    break;
                case MessageBoxImage.Question:
                    iconText = "?";
                    iconBrush = (Brush)FindResource("InfoBrush");
                    break;
                case MessageBoxImage.Information:
                default:
                    iconText = "i";
                    iconBrush = (Brush)FindResource("InfoBrush");
                    break;
            }

            IconTextBlock.Text = iconText;
            IconBorder.Background = iconBrush;
        }

        private void ConfigureButtons(MessageBoxButton buttons)
        {
            ButtonsPanel.Children.Clear();

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true);
                    _closeResult = MessageBoxResult.None;
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true);
                    AddButton("Cancel", MessageBoxResult.Cancel, isDefault: false, isCancel: true);
                    _closeResult = MessageBoxResult.Cancel;
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, isDefault: true);
                    AddButton("No", MessageBoxResult.No, isDefault: false, isCancel: true);
                    _closeResult = MessageBoxResult.No;
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Yes", MessageBoxResult.Yes, isDefault: true);
                    AddButton("No", MessageBoxResult.No, isDefault: false);
                    AddButton("Cancel", MessageBoxResult.Cancel, isDefault: false, isCancel: true);
                    _closeResult = MessageBoxResult.Cancel;
                    break;
                default:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true);
                    _closeResult = MessageBoxResult.None;
                    break;
            }
        }

        private void AddButton(string content, MessageBoxResult result, bool isDefault, bool isCancel = false)
        {
            Button button = new()
            {
                Content = content,
                MinWidth = 90,
                Margin = new Thickness(4, 0, 0, 0),
                Style = (Style)FindResource(isCancel ? "SecondaryButton" : "PrimaryButton"),
                IsDefault = isDefault,
                IsCancel = isCancel
            };

            button.Click += (_, _) => CloseWithResult(result);
            ButtonsPanel.Children.Add(button);
        }

        private void CloseWithResult(MessageBoxResult result)
        {
            Result = result;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Result = _closeResult;
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Result == MessageBoxResult.None && _closeResult != MessageBoxResult.None)
            {
                Result = _closeResult;
            }
        }
    }
}

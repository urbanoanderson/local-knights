using System.Windows.Controls;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Views
{
    public partial class ClubPreferencesView : UserControl
    {
        public ClubPreferencesView()
        {
            InitializeComponent();
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ClubPreferencesViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.CurrentPassword = passwordBox.Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ClubPreferencesViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.NewPassword = passwordBox.Password;
            }
        }

        private void ConfirmNewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ClubPreferencesViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmNewPassword = passwordBox.Password;
            }
        }
    }
}

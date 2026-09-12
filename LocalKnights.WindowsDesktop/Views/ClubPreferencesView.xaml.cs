using System.Windows.Controls;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Views
{
    public partial class ClubPreferencesView : UserControl
    {
        public ClubPreferencesView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ClubPreferencesViewModel oldViewModel)
            {
                oldViewModel.PasswordsCleared -= ClearPasswords;
            }

            if (e.NewValue is ClubPreferencesViewModel newViewModel)
            {
                newViewModel.PasswordsCleared += ClearPasswords;
            }
        }

        private void ClearPasswords()
        {
            CurrentPasswordInput.Clear();
            NewPasswordInput.Clear();
            ConfirmNewPasswordInput.Clear();
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

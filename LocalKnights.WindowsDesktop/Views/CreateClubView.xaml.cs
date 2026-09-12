using System.Windows.Controls;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Views
{
    /// <summary>
    /// Interaction logic for CreateClubView.xaml
    /// </summary>
    public partial class CreateClubView : UserControl
    {
        public CreateClubView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CreateClubViewModel oldViewModel)
            {
                oldViewModel.PasswordsCleared -= ClearPasswords;
            }

            if (e.NewValue is CreateClubViewModel newViewModel)
            {
                newViewModel.PasswordsCleared += ClearPasswords;
            }
        }

        private void ClearPasswords()
        {
            PasswordInput.Clear();
            ConfirmPasswordInput.Clear();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CreateClubViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CreateClubViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
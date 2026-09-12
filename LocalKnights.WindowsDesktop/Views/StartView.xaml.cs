using System.Windows.Controls;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Views
{
    /// <summary>
    /// Interaction logic for StartView.xaml
    /// </summary>
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is StartViewModel oldViewModel)
            {
                oldViewModel.PasswordCleared -= ClearPassword;
            }

            if (e.NewValue is StartViewModel newViewModel)
            {
                newViewModel.PasswordCleared += ClearPassword;
            }
        }

        private void ClearPassword()
        {
            PasswordInput.Clear();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is StartViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && DataContext is StartViewModel viewModel)
            {
                if (viewModel.SignInCommand.CanExecute(null))
                {
                    viewModel.SignInCommand.Execute(null);
                }
            }
        }
    }
}
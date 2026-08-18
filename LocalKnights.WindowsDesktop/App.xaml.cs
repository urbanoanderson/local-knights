using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LocalKnights.WindowsDesktop.Views;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
    }

    /// <summary>
    /// Performs application startup initialization.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ServiceProviderManager.ConfigureServices();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing services: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }

        try
        {
            MainWindow mainWindow = new MainWindow();

            MainWindowViewModel? viewModel = ServiceProviderManager.ServiceProvider.GetService<MainWindowViewModel>();
            if (viewModel == null)
            {
                throw new InvalidOperationException("MainWindowViewModel is not registered in the service provider.");
            }

            viewModel.Initialize();
            mainWindow.DataContext = viewModel;
            mainWindow.Closing += (_, _) => viewModel.OnWindowClosing();

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Critical error during startup: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}


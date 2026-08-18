using System.Windows;

namespace LocalKnights.WindowsDesktop.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing MainWindow: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}
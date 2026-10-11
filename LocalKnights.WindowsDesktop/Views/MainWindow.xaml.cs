using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace LocalKnights.WindowsDesktop.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            SourceInitialized += (_, _) => ApplyTitleBarTheme();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            Closed += (_, _) => SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing MainWindow: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(ApplyTitleBarTheme);
            return;
        }

        ApplyTitleBarTheme();
    }

    private void ApplyTitleBarTheme()
    {
        nint windowHandle = new WindowInteropHelper(this).Handle;
        if (windowHandle == nint.Zero)
            return;

        int useDarkMode = IsSystemDarkModeEnabled() ? 1 : 0;
        _ = DwmSetWindowAttribute(windowHandle, DwmwaUseImmersiveDarkMode, ref useDarkMode, sizeof(int));
    }

    private static bool IsSystemDarkModeEnabled()
    {
        using RegistryKey? personalizeKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return personalizeKey?.GetValue("AppsUseLightTheme") is int appsUseLightTheme && appsUseLightTheme == 0;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint windowHandle, int attribute, ref int attributeValue, int attributeSize);
}
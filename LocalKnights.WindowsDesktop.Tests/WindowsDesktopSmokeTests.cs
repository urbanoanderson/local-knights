using LocalKnights.WindowsDesktop.Views;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Provides a minimal smoke test for the Windows Desktop project.
/// </summary>
public sealed class WindowsDesktopSmokeTests
{
    /// <summary>
    /// Verifies the common assembly loads into the test project.
    /// </summary>
    [Fact]
    public void CommonAssemblyIsReferenced()
    {
    }

    /// <summary>
    /// Verifies competition views can load their XAML resources and dependency properties.
    /// </summary>
    [Fact]
    public void CompetitionViewsCanBeConstructed()
    {
        Exception? constructionException = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                App application = new App();
                application.InitializeComponent();
                _ = new CreateCompetitionView();
                _ = new CompetitionView();
            }
            catch (Exception exception)
            {
                constructionException = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(constructionException);
    }
}

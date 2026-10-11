using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Services;
using LocalKnights.Common.Services.Database;
using LocalKnights.WindowsDesktop.ViewModels;
using LocalKnights.WindowsDesktop.Services;

namespace LocalKnights.WindowsDesktop;

/// <summary>
/// Manages DependencyInjection and IServiceProvider instance
/// </summary>
public static class ServiceProviderManager
{
    /// <summary>
    /// Globally available instance of application's service provider.
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    /// <summary>
    /// Registers application dependencies.
    /// </summary>
    public static void ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        // Add logging configuration only for debug builds
        services.AddLogging(builder =>
        {
            #if DEBUG
                builder.AddConsole();
            #endif
        });

        services.AddSingleton<ILogger>(serviceProvider => serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(App)));

        // Register Common services
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IClubService, ClubService>();
        services.AddSingleton<IChessDotComApiService, ChessDotComApiService>();
        services.AddSingleton<ILichessApiService, LichessApiService>();
        services.AddSingleton<IFideApiService, FideApiService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<ICompetitionService, CompetitionService>();

        // Register UI services and Page ViewModels
        services.AddSingleton<IUISettingsService, UISettingsService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<IPageNavigationService, PageNavigationService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<StartViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddTransient<CreateClubViewModel>();
        services.AddTransient<ClubPreferencesViewModel>();
        services.AddTransient<PlayersViewModel>();
        services.AddTransient<CreatePlayerViewModel>();
        services.AddTransient<PlayerViewModel>();
        services.AddTransient<GamesViewModel>();
        services.AddTransient<GameViewModel>();
        services.AddTransient<CreateGameViewModel>();
        services.AddTransient<CompetitionsViewModel>();
        services.AddTransient<CreateCompetitionViewModel>();
        services.AddTransient<CompetitionViewModel>();

        // Build usable ServiceProvider
        ServiceProvider = services.BuildServiceProvider();
    }
}


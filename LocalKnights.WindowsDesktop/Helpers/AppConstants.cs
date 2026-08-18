using System.Reflection;

namespace LocalKnights.WindowsDesktop.Helpers
{
    /// <summary>
    /// Provides application-wide constant values.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Application version loaded from executing assembly.
        /// </summary>

        public static readonly Version AppVersion = Assembly.GetExecutingAssembly().GetName()?.Version ?? new Version(0, 0, 0, 0);

        /// <summary>
        /// The application name to be displayed.
        /// </summary>
        public const string AppDisplayName = "Local Knights";

        /// <summary>
        /// The default icon for a player.
        /// </summary>
        public const string DefaultPlayerIcon = "👤";

        /// <summary>
        /// The default icon for a game.
        /// </summary>
        public const string DefaultGameIcon = "🏁";

        /// <summary>
        /// The default icon for a competition.
        /// </summary>
        public const string DefaultCompetitionIcon = "🏆";

        /// <summary>
        /// The default icon for club preferences.
        /// </summary>
        public const string DefaultPreferencesIcon = "🔧";

        /// <summary>
        /// The default icon for search.
        /// </summary>
        public const string DefaultSearchIcon = "🔍";

        /// <summary>
        /// The default icon for bullet time format.
        /// </summary>
        public const string BulletTimeFormatIcon = "🚅";

        /// <summary>
        /// The default icon for blitz time format.
        /// </summary>
        public const string BlitzTimeFormatIcon = "⚡";

        /// <summary>
        /// The default icon for rapid time format.
        /// </summary>
        public const string RapidTimeFormatIcon = "⏱️";

        /// <summary>
        /// The default icon for classical time format.
        /// </summary>
        public const string ClassicalTimeFormatIcon = "🎩";
    }
}

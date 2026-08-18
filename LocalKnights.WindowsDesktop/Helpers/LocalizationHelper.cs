using System.Globalization;
using System.Resources;
using LocalKnights.WindowsDesktop.ViewModels;

namespace LocalKnights.WindowsDesktop.Helpers
{
    /// <summary>
    /// Provides access to localized application strings.
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>
        /// Loads application resource strings from the main assembly.
        /// </summary>
        private static readonly ResourceManager ResourceManager = new ResourceManager("LocalKnights.WindowsDesktop.Resources.Resources", typeof(MainWindowViewModel).Assembly);

        /// <summary>
        /// Gets a localized string for the current culture, or falls back to English if not found. If the key is not found in either culture, returns an empty string.
        /// </summary>
        /// <param name="key">The localized string key</param>
        /// <returns>The localized string</returns>
        public static string GetString(string key)
        {
            // Try current culture
            string? value = ResourceManager.GetString(key, CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(value))
                return value;

            // Fallback to English
            return ResourceManager.GetString(key, CultureInfo.GetCultureInfo("en")) ?? string.Empty;
        }

        /// <summary>
        /// Gets a localized string and add onto a format string inside the first {0} param, then format the rest of the params as normal.
        /// This allows for more flexible formatting in languages that may have different word order.
        /// </summary>
        /// <param name="key">The localized string key</param>
        /// <param name="format">The format string</param>
        /// <param name="args">The arguments for the format string</param>
        /// <returns>The formatted localized string</returns>
        public static string GetStringFormat(string key, string format, params object[] args)
        {
            string localizedString = GetString(key);
            object[] allArgs = new object[args.Length + 1];
            allArgs[0] = localizedString;
            Array.Copy(args, 0, allArgs, 1, args.Length);
            return string.Format(format, allArgs);
        }
    }
}
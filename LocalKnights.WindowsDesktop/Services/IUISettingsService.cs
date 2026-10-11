using System;
using System.IO;
using System.Text.Json;
using LocalKnights.Common.Models;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Provides functionality to load and save user UI application settings to a JSON file in the local application data directory.
    /// </summary>
    public interface IUISettingsService
    {
        /// <summary>
        /// Gets the currently loaded application settings.
        /// </summary>
        UISettings CurrentAppSettings { get; }

        /// <summary>
        /// Occurs after the dark mode toggle is changed.
        /// </summary>
        event Action<bool>? OnSetDarkModeToggle;

        /// <summary>
        /// Occurs after the application language is changed.
        /// </summary>
        event Action<string>? OnSetLanguage;

        /// <summary>
        /// Occurs after the page size is changed.
        /// </summary>
        event Action<int>? OnSetPageSize;

        /// <summary>
        /// Loads the user application settings from a JSON file. If the file does not exist or is invalid, default settings are created and saved.
        /// </summary>
        /// <param name="defaultDarkMode"></param>
        /// <returns>User application settings.</returns>
        UISettings Load(bool defaultDarkMode);

        /// <summary>
        /// Updates the dark mode setting, raises its change event, and persists the updated settings.
        /// </summary>
        /// <param name="isDarkMode">The new dark mode toggle value.</param>
        void SetDarkModeToggle(bool isDarkMode);

        /// <summary>
        /// Updates the language setting, raises its change event, and persists the updated settings.
        /// </summary>
        /// <param name="languageCode">The new UI language code.</param>
        void SetLanguage(string languageCode);

        /// <summary>
        /// Updates the page size setting, raises its change event, and persists the updated settings.
        /// </summary>
        /// <param name="pageSize">The new page size.</param>
        void SetPageSize(int pageSize);

        /// <summary>
        /// Saves the user application settings to a JSON file in the local application data directory. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="settings">The user application settings to save.</param>
        void Save(UISettings settings);

        /// <summary>
        /// Detects the current Windows theme preference (Light or Dark).
        /// </summary>
        /// <returns>true when dark mode is preferred by user's system; otherwise false.</returns>
        bool IsSystemDarkModeEnabled();
    }
}

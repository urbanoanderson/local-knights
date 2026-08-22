using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Services;
using Microsoft.Win32;

namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Default implementation of <see cref="IUISettingsService"/> that loads and saves user
    /// application settings to a JSON file in the local application data directory.
    /// </summary>
    public sealed class UISettingsService : IUISettingsService
    {
        private readonly ILogger _logger;

        private readonly IFileSystemService _fileSystemService;

        private readonly string _settingsFilePath;

        /// <inheritdoc/>
        public UISettings CurrentAppSettings { get; private set; } = new UISettings
        {
            IsDarkMode = false,
            LanguageCode = "en",
            PageSize = 50
        };

        /// <inheritdoc/>
        public event Action<bool>? OnSetDarkModeToggle;

        /// <inheritdoc/>
        public event Action<string>? OnSetLanguage;

        /// <inheritdoc/>
        public event Action<int>? OnSetPageSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="UISettingsService"/> class.
        /// </summary>
        /// <param name="fileSystemService">The file system service for performing file operations.</param>
        /// <param name="logger">The logger for recording service events.</param>
        public UISettingsService(IFileSystemService fileSystemService, ILogger logger)
        {
            _fileSystemService = fileSystemService;
            _logger = logger;

            string appDataDir = _fileSystemService.GetAppDataDirectory();
            _settingsFilePath = Path.Combine(appDataDir, "settings.json");
        }

        /// <inheritdoc/>
        public UISettings Load(bool defaultDarkMode)
        {
            UISettings defaults = new UISettings
            {
                IsDarkMode = defaultDarkMode,
                LanguageCode = "en",
                PageSize = 50
            };

            try
            {
                if (!_fileSystemService.FileExists(_settingsFilePath))
                {
                    CurrentAppSettings = defaults;
                    Save(CurrentAppSettings);
                    return CurrentAppSettings;
                }

                string json = _fileSystemService.ReadAllText(_settingsFilePath);
                UISettings? loaded = JsonSerializer.Deserialize<UISettings>(json);
                if (loaded == null)
                {
                    CurrentAppSettings = defaults;
                    Save(CurrentAppSettings);
                    return CurrentAppSettings;
                }

                UISettings normalized = Normalize(loaded, defaults);
                CurrentAppSettings = normalized;
                Save(CurrentAppSettings);

                return CurrentAppSettings;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to load user application settings from '{_settingsFilePath}'. Using default settings. Possible causes: file not found, invalid JSON format, or deserialization error. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                CurrentAppSettings = defaults;
                Save(CurrentAppSettings);
                return CurrentAppSettings;
            }
        }

        /// <inheritdoc/>
        public void SetDarkModeToggle(bool isDarkMode)
        {
            if (CurrentAppSettings.IsDarkMode == isDarkMode)
            {
                return;
            }

            CurrentAppSettings = new UISettings
            {
                IsDarkMode = isDarkMode,
                LanguageCode = CurrentAppSettings.LanguageCode,
                PageSize = CurrentAppSettings.PageSize
            };

            OnSetDarkModeToggle?.Invoke(CurrentAppSettings.IsDarkMode);
            Save(CurrentAppSettings);
        }

        /// <inheritdoc/>
        public void SetLanguage(string languageCode)
        {
            UISettings normalized = Normalize(new UISettings
            {
                IsDarkMode = CurrentAppSettings.IsDarkMode,
                LanguageCode = languageCode,
                PageSize = CurrentAppSettings.PageSize
            }, CurrentAppSettings);

            if (CurrentAppSettings.LanguageCode == normalized.LanguageCode)
            {
                return;
            }

            CurrentAppSettings = normalized;
            OnSetLanguage?.Invoke(CurrentAppSettings.LanguageCode);
            Save(CurrentAppSettings);
        }

        /// <inheritdoc/>
        public void SetPageSize(int pageSize)
        {
            UISettings normalized = Normalize(new UISettings
            {
                IsDarkMode = CurrentAppSettings.IsDarkMode,
                LanguageCode = CurrentAppSettings.LanguageCode,
                PageSize = pageSize
            }, CurrentAppSettings);

            if (CurrentAppSettings.PageSize == normalized.PageSize)
            {
                return;
            }

            CurrentAppSettings = normalized;
            OnSetPageSize?.Invoke(CurrentAppSettings.PageSize);
            Save(CurrentAppSettings);
        }

        /// <inheritdoc/>
        public void Save(UISettings settings)
        {
            try
            {
                UISettings normalized = Normalize(settings, settings);
                CurrentAppSettings = normalized;
                string? directory = Path.GetDirectoryName(_settingsFilePath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    _fileSystemService.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _fileSystemService.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to save user application settings to '{_settingsFilePath}'. Changes will not be persisted. Possible causes: insufficient permissions, disk errors, or serialization issues. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                // Keep app usable if persistence fails.
            }
        }

        /// <summary>
        ///  Normalizes the user application settings by validating and correcting individual properties as needed.
        ///  If any property is invalid, it is replaced with the corresponding default value.
        /// </summary>
        /// <param name="candidate">The user application settings to normalize.</param>
        /// <param name="defaults">The default user application settings.</param>
        /// <returns>The normalized user application settings.</returns>
        private static UISettings Normalize(UISettings candidate, UISettings defaults)
        {
            int pageSize = candidate.PageSize;
            if (pageSize != 10 && pageSize != 50 && pageSize != 100)
            {
                pageSize = defaults.PageSize;
            }

            string languageCode = candidate.LanguageCode;
            if (languageCode != "en" && languageCode != "pt-BR" && languageCode != "es" && languageCode != "fr")
            {
                languageCode = defaults.LanguageCode;
            }

            return new UISettings
            {
                IsDarkMode = candidate.IsDarkMode,
                LanguageCode = languageCode,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public bool IsSystemDarkModeEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    object? value = key.GetValue("AppsUseLightTheme");
                    if (value is int intValue)
                    {
                        return intValue == 0; //In the regkey 0 = dark, 1 = light
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not detect system theme, defaulting to light mode. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
            }

            return false; // Default to light
        }
    }
}

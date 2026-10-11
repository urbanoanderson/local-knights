namespace LocalKnights.WindowsDesktop.Services
{
    /// <summary>
    /// Represents persisted application UI settings selected by the user.
    /// </summary>
    public sealed class UISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether dark mode is enabled.
        /// </summary>
        public bool IsDarkMode { get; set; }

        /// <summary>
        /// Gets or sets the selected UI language code.
        /// </summary>
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Gets or sets the number of items shown per page.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
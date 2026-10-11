using System;
using System.Globalization;
using System.Windows.Data;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.Converters
{
    /// <summary>
    /// Converts enum values to localized strings using keys in the format &lt;EnumName_EnumValue&gt;.
    /// Falls back to the enum value string when no localization key is found.
    /// </summary>
    public class DomainEnumStringConverter : IValueConverter
    {
        /// <summary>
        /// Gets the shared singleton instance of this converter.
        /// </summary>
        public static DomainEnumStringConverter Instance { get; } = new DomainEnumStringConverter();

        /// <summary>
        /// Convenience method for direct conversion without needing to specify unused parameters.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>The localized enum string or enum value string.</returns>
        public string Convert(object? value)
        {
            return (string)this.Convert(value, typeof(string), null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts an enum value to a localized string.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>The localized enum string or enum value string.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value is not Enum enumValue)
            {
                return string.Empty;
            }

            string enumName = enumValue.GetType().Name;
            string key = $"{enumName}_{enumValue}";
            string localized = LocalizationHelper.GetString(key);

            return string.IsNullOrWhiteSpace(localized) ? enumValue.ToString() : localized;
        }

        /// <summary>
        /// Reverse conversion is not supported.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>This method does not return.</returns>
        /// <exception cref="NotSupportedException">Always thrown because reverse conversion is not supported.</exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("DomainEnumStringConverter does not support reverse conversion.");
        }
    }
}
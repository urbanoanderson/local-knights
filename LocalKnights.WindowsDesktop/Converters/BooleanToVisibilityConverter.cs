using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LocalKnights.WindowsDesktop.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility enum value.
    /// True maps to Visibility.Visible, False maps to Visibility.Collapsed.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property (not used).</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>Visibility.Visible if value is true; Visibility.Collapsed otherwise.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value for two-way binding.
        /// </summary>
        /// <param name="value">The Visibility value to convert back.</param>
        /// <param name="targetType">The type of the binding target property (not used).</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>True if value is Visibility.Visible; false otherwise.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}

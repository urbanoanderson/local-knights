using System;
using System.Globalization;
using System.Windows.Data;

namespace LocalKnights.WindowsDesktop.Converters
{
    /// <summary>
    /// Converts a boolean value to its inverse (true becomes false, false becomes true).
    /// This converter is useful for binding to UI elements that need to show the opposite state of a boolean property.
    /// </summary>
    public class BooleanToInverseConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property (not used).</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>The inverse of the input boolean value; returns true if the input is not a boolean.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        /// <summary>
        /// Converts a boolean value back to its inverse for two-way binding.
        /// </summary>
        /// <param name="value">The boolean value to convert back.</param>
        /// <param name="targetType">The type of the binding target property (not used).</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>The inverse of the input boolean value; returns true if the input is not a boolean.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }
    }
}

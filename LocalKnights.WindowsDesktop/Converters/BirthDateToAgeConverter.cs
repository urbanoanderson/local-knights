using System;
using System.Globalization;
using System.Windows.Data;

namespace LocalKnights.WindowsDesktop.Converters
{
    /// <summary>
    /// Converts a player's BirthDate to an age in years. Returns empty string when BirthDate is null.
    /// </summary>
    public class BirthDateToAgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            DateTime birthDate;

            if (value is DateTime)
            {
                birthDate = (DateTime)value;
            }
            else
            {
                if (!DateTime.TryParse(value.ToString(), out birthDate))
                    return string.Empty;
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;

            return age >= 0 ? age.ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

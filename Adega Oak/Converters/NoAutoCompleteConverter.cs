using System;
using System.Windows.Data;

namespace Adega_Oak.Converters
{
    public class NoAutoCompleteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Retorna o valor como está, sem modificação
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Apenas retorna o que foi digitado, sem autocomplete
            return value ?? string.Empty;
        }
    }
}

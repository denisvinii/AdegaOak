using System.Windows;
using System.Windows.Data;

namespace Adega_Oak.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int paramValue))
        {
            return intValue == paramValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

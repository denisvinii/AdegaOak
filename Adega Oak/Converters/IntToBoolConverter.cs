using System.Windows.Data;

namespace Adega_Oak.Converters;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int paramValue))
        {
            return intValue == paramValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramStr && int.TryParse(paramStr, out int paramValue))
        {
            return boolValue ? paramValue : 0;
        }
        return 0;
    }
}

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Adega_Oak.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLower();
        if (status != null && status.Contains("online"))
            return new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)); // Verde
        else
            return new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)); // Vermelho
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

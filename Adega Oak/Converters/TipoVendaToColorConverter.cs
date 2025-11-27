using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Adega_Oak.Converters;

public class TipoVendaToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        // Always return fixed orange color for buttons; selection indicated only by shadow converter
        return new SolidColorBrush(Color.FromRgb(255, 165, 0)); // #FFA500
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}

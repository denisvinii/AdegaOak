using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Adega_Oak.Converters;

/// <summary>
/// Converte porcentagem de margem para cor: Verde (>0) > Amarelo (=0) > Vermelho (<0)
/// </summary>
public class MarginToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal margin)
        {
            return margin switch
            {
                > 0 => new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)),    // Verde: #4CAF50
                < 0 => new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)),     // Vermelho: #F44336
                _ => new SolidColorBrush(Color.FromArgb(255, 255, 183, 77))      // Amarelo/Orange: #FFB74D
            };
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

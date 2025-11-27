using System.Globalization;
using System.Windows.Data;

namespace Adega_Oak.Converters
{
    public class TipoVendaNotAtacadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Retorna false (desabilitado) se for "Atacado", true (habilitado) caso contrário
            return value is string str && str != "Atacado";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

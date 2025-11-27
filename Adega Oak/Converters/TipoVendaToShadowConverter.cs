using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace Adega_Oak.Converters
{
    public class TipoVendaToShadowConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var tipoSelecionado = value as string;
            var tipoParametro = parameter as string;
            if (tipoSelecionado == null || tipoParametro == null)
                return null!;

            if (tipoSelecionado == tipoParametro)
            {
                return new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.35
                };
            }

            return null!;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

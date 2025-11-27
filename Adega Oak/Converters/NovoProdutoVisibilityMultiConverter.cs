using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Adega_Oak.Converters
{
    public class NovoProdutoVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return Visibility.Collapsed;

            bool isEntrada = values[0] is bool b && b;
            var produtoSelecionado = values[1];

            // Mostra os campos de novo produto apenas se for Entrada e nenhum produto existente estiver selecionado
            return (isEntrada && produtoSelecionado == null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;
using System.Windows.Data;

namespace Adega_Oak.Converters;

/// <summary>
/// Validador de preço mínimo: retorna true se preço < custo mínimo (10% acima do custo)
/// </summary>
public class InvalidPriceValidator : IMultiValueConverter
{
    private const decimal MARGEM_MINIMA = 1.10m;

    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 3)
            return false;

        if (decimal.TryParse(values[0]?.ToString(), out var preco) &&
            decimal.TryParse(values[1]?.ToString(), out var custo) &&
            int.TryParse(values[2]?.ToString(), out var quantidade))
        {
            var custoMinimo = (custo * quantidade) * MARGEM_MINIMA;
            return preco < custoMinimo;
        }

        return false;
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

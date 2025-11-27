using System.Windows;
using System.Windows.Controls;

namespace Adega_Oak.Views
{
    public partial class SaldoView : UserControl
    {
        public SaldoView()
        {
            InitializeComponent();
        }

        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Features.Saldo.SaldoViewModel vm)
            {
                vm.CarregarSaldo();
            }
        }
    }
}


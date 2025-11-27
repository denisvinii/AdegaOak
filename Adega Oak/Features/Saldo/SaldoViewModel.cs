using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Adega_Oak.Features.Saldo
{
    public class SaldoDiario
    {
        public DateTime Data { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
    }

    public class SaldoViewModel : INotifyPropertyChanged
    {
        private readonly SaldoRepository _saldoRepository;
        public decimal CapitalEmpresa { get; private set; }
        public decimal InvestimentoPorFora { get; private set; }
        public decimal Saldo { get; private set; }
        public ObservableCollection<SaldoDiario> MovimentacoesDiarias { get; } = new();
        public ObservableCollection<int> Meses { get; } = new(Enumerable.Range(1, 12));
        public int MesSelecionado { get; set; } = DateTime.Now.Month;
        public int AnoSelecionado { get; set; } = DateTime.Now.Year;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ReloadCommand { get; }

        public SaldoViewModel(SaldoRepository saldoRepository)
        {
            _saldoRepository = saldoRepository;
            ReloadCommand = new RelayCommand(CarregarSaldo);
            CarregarSaldo();
        }

        public void CarregarSaldo()
        {
            var saldo = _saldoRepository.ObterSaldoGeral();
            CapitalEmpresa = saldo.CapitalEmpresa;
            InvestimentoPorFora = saldo.InvestimentoPorFora;
            Saldo = saldo.Saldo;
            OnPropertyChanged(nameof(CapitalEmpresa));
            OnPropertyChanged(nameof(InvestimentoPorFora));
            OnPropertyChanged(nameof(Saldo));
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Adega_Oak.Features.Historico;

public partial class HistoricoViewModel : ObservableObject
{
    private readonly HistoricoRepository _historicoRepository;

    [ObservableProperty]
    private ObservableCollection<MainRepository.Movimentacao> historico = new();

    [ObservableProperty]
    private MainRepository.Movimentacao? movimentacaoSelecionada;

    [ObservableProperty]
    private HistoricoRepository.RelatorioMovimentacaoDiaria? relatorioDiario;

    [ObservableProperty]
    private DateTime dataSelecionada = DateTime.Today;

    public HistoricoViewModel(HistoricoRepository historicoRepository)
    {
        _historicoRepository = historicoRepository;
        CarregarHistorico();
    }

    public void CarregarHistorico()
    {
        Historico = new ObservableCollection<MainRepository.Movimentacao>(_historicoRepository.CarregarHistorico());
    }

    [RelayCommand]
    private void ExcluirMovimentacao(int productId)
    {
        var movimentacao = Historico.FirstOrDefault(m => m.ProductId == productId);
        if (movimentacao != null)
        {
            _historicoRepository.ExcluirMovimentacao(productId, movimentacao.Data);
            CarregarHistorico(); // Atualiza a lista após exclusão
        }
    }

    [RelayCommand]
    private void GerarRelatorioDiario()
    {
        RelatorioDiario = _historicoRepository.GerarRelatorioDiario(DataSelecionada);
    }

    [RelayCommand]
    private void FecharRelatorio()
    {
        RelatorioDiario = null;
    }
}
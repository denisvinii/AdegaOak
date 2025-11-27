using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Adega_Oak.Features.Historico;

public partial class HistoricoViewModel : ObservableObject
{
    #region Campos
    private readonly HistoricoRepository _historicoRepository;
    private string _tipoVendaFiltro = "Todos";
    #endregion

    #region Propriedades
    [ObservableProperty]
    private ObservableCollection<MainRepository.Movimentacao> historico = new();

    [ObservableProperty]
    private MainRepository.Movimentacao? movimentacaoSelecionada;

    [ObservableProperty]
    private HistoricoRepository.RelatorioMovimentacaoDiaria? relatorioDiario;

    [ObservableProperty]
    private DateTime dataSelecionada = DateTime.Today;

    // Novo: Filtro de tipo de venda
    public string TipoVendaFiltro
    {
        get => _tipoVendaFiltro;
        set
        {
            if (SetProperty(ref _tipoVendaFiltro, value))
            {
                CarregarHistorico();
            }
        }
    }

    [ObservableProperty]
    private decimal totalVarejoDia = 0;

    [ObservableProperty]
    private decimal totalAtacadoDia = 0;

    public ObservableCollection<string> TiposVendaFiltro { get; } = new() { "Todos", "Varejo", "Atacado" };
    #endregion

    #region Construtor
    public HistoricoViewModel(HistoricoRepository historicoRepository)
    {
        _historicoRepository = historicoRepository;
        CarregarHistorico();
    }
    #endregion

    #region Métodos
    public void CarregarHistorico()
    {
        var todosMovimentos = _historicoRepository.CarregarHistorico();
        
        // Aplicar filtro de tipo de venda
        if (this.TipoVendaFiltro == "Varejo")
        {
            todosMovimentos = todosMovimentos.Where(m => m.TipoVenda == "Varejo" || string.IsNullOrEmpty(m.TipoVenda)).ToList();
        }
        else if (this.TipoVendaFiltro == "Atacado")
        {
            todosMovimentos = todosMovimentos.Where(m => m.TipoVenda == "Atacado").ToList();
        }
        
        Historico = new ObservableCollection<MainRepository.Movimentacao>(todosMovimentos);
    }
    #endregion

    #region Comandos
    [RelayCommand]
    private void ExcluirMovimentacao(int productId)
    {
        // Exclui movimentação pelo ProductId
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
        // Gera relatório diário para a data selecionada
        RelatorioDiario = _historicoRepository.GerarRelatorioDiario(DataSelecionada);
        
        // Calcular totais por tipo de venda
        if (RelatorioDiario != null)
        {
            var vendas = Historico.Where(m => m.Data.Date == DataSelecionada.Date && m.Tipo == "Saída").ToList();
            
            TotalVarejoDia = vendas
                .Where(m => m.TipoVenda == "Varejo" || string.IsNullOrEmpty(m.TipoVenda))
                .Sum(m => m.ValorTotal);
            
            TotalAtacadoDia = vendas
                .Where(m => m.TipoVenda == "Atacado")
                .Sum(m => m.ValorTotal);
        }
    }

    [RelayCommand]
    private void FecharRelatorio()
    {
        // Fecha o relatório diário
        RelatorioDiario = null;
    }
    #endregion
}
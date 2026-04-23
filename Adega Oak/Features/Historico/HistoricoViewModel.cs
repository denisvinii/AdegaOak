using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Adega_Oak.Features.Historico;

public partial class HistoricoViewModel : ObservableObject
{
    #region Campos
    private readonly HistoricoRepository _historicoRepository;
    private readonly MainRepository _mainRepository;
    private string _tipoVendaFiltro = "Todos";
    private string _tipoMovimentacaoFiltro = "Todos";
    private string _responsavelFiltro = "Todos";
    private List<MainRepository.Movimentacao> _todosMovimentos = new();
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

    // Filtro de tipo de venda
    public string TipoVendaFiltro
    {
        get => _tipoVendaFiltro;
        set
        {
            if (SetProperty(ref _tipoVendaFiltro, value))
            {
                AplicarFiltros();
            }
        }
    }

    // Novo: Filtro de tipo de movimentação (Entrada/Saída)
    public string TipoMovimentacaoFiltro
    {
        get => _tipoMovimentacaoFiltro;
        set
        {
            if (SetProperty(ref _tipoMovimentacaoFiltro, value))
            {
                AplicarFiltros();
            }
        }
    }

    // Novo: Filtro de responsável
    public string ResponsavelFiltro
    {
        get => _responsavelFiltro;
        set
        {
            if (SetProperty(ref _responsavelFiltro, value))
            {
                AplicarFiltros();
            }
        }
    }

    [ObservableProperty]
    private decimal totalVarejoDia = 0;

    [ObservableProperty]
    private decimal totalAtacadoDia = 0;

    // Novo: Totais de entrada e saída
    [ObservableProperty]
    private decimal totalEntradas = 0;

    [ObservableProperty]
    private decimal totalSaidas = 0;

    [ObservableProperty]
    private decimal totalGeral = 0;

    public ObservableCollection<string> TiposVendaFiltro { get; } = new() { "Todos", "Varejo", "Atacado" };
    public ObservableCollection<string> TiposMovimentacaoFiltro { get; } = new() { "Todos", "Entrada", "Saída" };
    
    [ObservableProperty]
    private ObservableCollection<string> responsavelFiltroOpcoes = new();
    #endregion

    #region Construtor
    public HistoricoViewModel(HistoricoRepository historicoRepository, MainRepository mainRepository)
    {
        _historicoRepository = historicoRepository;
        _mainRepository = mainRepository;
        CarregarResponsaveis();
        CarregarHistorico();
    }
    #endregion

    #region Métodos
    public void CarregarHistorico()
    {
        _todosMovimentos = _historicoRepository.CarregarHistorico();
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var movimentosFiltrados = _todosMovimentos;

        // Filtro de tipo de venda
        if (TipoVendaFiltro == "Varejo")
        {
            movimentosFiltrados = movimentosFiltrados
                .Where(m => m.TipoVenda == "Varejo" || string.IsNullOrEmpty(m.TipoVenda))
                .ToList();
        }
        else if (TipoVendaFiltro == "Atacado")
        {
            movimentosFiltrados = movimentosFiltrados
                .Where(m => m.TipoVenda == "Atacado")
                .ToList();
        }

        // Filtro de tipo de movimentação (Entrada/Saída)
        if (TipoMovimentacaoFiltro == "Entrada")
        {
            movimentosFiltrados = movimentosFiltrados
                .Where(m => m.Tipo == "Entrada")
                .ToList();
        }
        else if (TipoMovimentacaoFiltro == "Saída")
        {
            movimentosFiltrados = movimentosFiltrados
                .Where(m => m.Tipo == "Saída")
                .ToList();
        }

        // Filtro de responsável
        if (ResponsavelFiltro != "Todos")
        {
            movimentosFiltrados = movimentosFiltrados
                .Where(m => m.Responsavel == ResponsavelFiltro)
                .ToList();
        }

        Historico = new ObservableCollection<MainRepository.Movimentacao>(movimentosFiltrados);
        CalcularTotais();
    }

    private void CalcularTotais()
    {
        TotalEntradas = Historico
            .Where(m => m.Tipo == "Entrada")
            .Sum(m => m.ValorTotal);

        TotalSaidas = Historico
            .Where(m => m.Tipo == "Saída")
            .Sum(m => m.ValorTotal);

        TotalGeral = TotalEntradas + TotalSaidas;
    }

    private void CarregarResponsaveis()
    {
        var responsaveis = _mainRepository.CarregarResponsaveis();
        ResponsavelFiltroOpcoes = new ObservableCollection<string>(new[] { "Todos" }.Concat(responsaveis));
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
        
        // Calcular totais por tipo de venda usando os dados do relatório (não do Historico que pode estar filtrado)
        if (RelatorioDiario != null)
        {
            TotalVarejoDia = RelatorioDiario.Movimentacoes
                .Where(m => m.TipoVenda == "Varejo" || string.IsNullOrEmpty(m.TipoVenda))
                .Sum(m => m.ValorTotal);
            
            TotalAtacadoDia = RelatorioDiario.Movimentacoes
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
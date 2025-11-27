using Adega_Oak.Features.Estoque;
using Adega_Oak.Features.Historico;
using Adega_Oak.Repositories;
using Adega_Oak.Services;
using Adega_Oak.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace Adega_Oak.Features.MainWindow;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly MainRepository _mainRepository;
    private readonly EstoqueViewModel _estoqueViewModel;
    private readonly HistoricoViewModel _historicoViewModel;
    private readonly DatabaseService _databaseService;
    private readonly InicioView _inicioView;
    private readonly EstoqueView _estoqueView;
    private readonly HistoricoView _historicoView;

    [ObservableProperty]
    private ObservableCollection<ProdutoSelecionadoItem> produtosSelecionados = new();

    [ObservableProperty]
    private ObservableCollection<MainRepository.Produto> produtos = new();

    [ObservableProperty]
    private ObservableCollection<MainRepository.Produto> produtosFiltrados = new();

    [ObservableProperty]
    private MainRepository.Produto? produtoSelecionado;

    [ObservableProperty]
    private string quantidade = "";

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string tipoMovimentacao = "Saída";

    [ObservableProperty]
    private string? tipoSaida;

    [ObservableProperty]
    private bool isSaida;

    [ObservableProperty]
    private ObservableCollection<string> responsaveis = new();

    [ObservableProperty]
    private string? responsavelSelecionado;

    [ObservableProperty]
    private decimal valorTotalSelecionado;

    [ObservableProperty]
    private string novoProdutoBebida = "";

    [ObservableProperty]
    private string novoProdutoTamanho = "";

    [ObservableProperty]
    private string novoProdutoMaterial = "";

    [ObservableProperty]
    private string valorUnitario = "0";

    public ObservableCollection<string> TiposMovimentacao { get; } = new() { "Entrada", "Saída" };
    public ObservableCollection<string> TiposSaida { get; } = new() { "Consumo", "Quebra", "Vencimento" };

    public MainWindowViewModel(
        MainRepository mainRepository,
        EstoqueViewModel estoqueViewModel,
        HistoricoViewModel historicoViewModel,
        DatabaseService databaseService)
    {
        _mainRepository = mainRepository;
        _estoqueViewModel = estoqueViewModel;
        _historicoViewModel = historicoViewModel;
        _databaseService = databaseService;

        _inicioView = new InicioView { DataContext = this };
        _estoqueView = new EstoqueView { DataContext = _estoqueViewModel };
        _historicoView = new HistoricoView { DataContext = _historicoViewModel };

        CurrentView = _inicioView;

        CarregarProdutos();
        CarregarResponsaveis();
        
        // Inicializa como Saída
        TipoMovimentacao = "Saída";
        // Inicializa ProdutosFiltrados com produtos com estoque
        OnTipoMovimentacaoChanged(TipoMovimentacao);
    }

    [RelayCommand]
    private void CarregarProdutos()
    {
        Produtos = new ObservableCollection<MainRepository.Produto>(_mainRepository.CarregarProdutos());
    }

    private void CarregarResponsaveis()
    {
        Responsaveis = new ObservableCollection<string>(_mainRepository.CarregarResponsaveis());
    }

    private async Task<MainRepository.Produto?> CriarNovoProduto()
    {
        if (string.IsNullOrWhiteSpace(NovoProdutoBebida) || 
            string.IsNullOrWhiteSpace(NovoProdutoTamanho) || 
            string.IsNullOrWhiteSpace(NovoProdutoMaterial))
        {
            return null;
        }

        var novoProduto = await Task.Run(() => _mainRepository.AdicionarProduto(new MainRepository.Produto
        {
            Bebida = NovoProdutoBebida,
            Tamanho = NovoProdutoTamanho,
            Material = NovoProdutoMaterial
        }));

        if (novoProduto != null)
        {
            CarregarProdutos();
            return novoProduto;
        }

        return null;
    }

    [RelayCommand]
    private async Task AdicionarProdutoAsync()
    {
        if (!int.TryParse(Quantidade, out int qtd) || qtd <= 0)
        {
            MessageBox.Show("Por favor, informe uma quantidade válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (TipoMovimentacao == "Entrada" && (!decimal.TryParse(ValorUnitario, out decimal valor) || valor <= 0))
        {
            MessageBox.Show("Por favor, informe um valor unitário válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MainRepository.Produto? produto = null;
        decimal valorFinal = 0;

        if (TipoMovimentacao == "Entrada")
        {
            if (ProdutoSelecionado == null)
            {
                // Tenta criar novo produto se os campos estiverem preenchidos
                produto = await CriarNovoProduto();
                if (produto == null)
                {
                    MessageBox.Show("Por favor, preencha todos os campos do produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                produto = ProdutoSelecionado;
            }

            // Em entrada, sempre usa o valor informado
            valorFinal = decimal.Parse(ValorUnitario);
        }
        else // Saída
        {
            if (ProdutoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione um produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            produto = ProdutoSelecionado;
            
            var produtoEstoque = _estoqueViewModel.Estoque
                .FirstOrDefault(e => e.ProductId == produto.ProductId);

            if (produtoEstoque == null || produtoEstoque.Quantidade <= 0)
            {
                MessageBox.Show("Este produto não possui estoque disponível.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (produtoEstoque.Quantidade < qtd)
            {
                MessageBox.Show($"Estoque insuficiente. Quantidade disponível: {produtoEstoque.Quantidade}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TipoSaida))
            {
                MessageBox.Show("Por favor, selecione o tipo de saída.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            valorFinal = produtoEstoque.Valor;
        }

        ProdutosSelecionados.Add(new ProdutoSelecionadoItem
        {
            Descricao = produto.Descricao,
            Quantidade = qtd,
            Produto = produto,
            TipoMovimentacao = TipoMovimentacao,
            TipoSaida = IsSaida ? TipoSaida : null,
            ValorUnitario = valorFinal
        });

        AtualizarValorTotal();

        // Limpar campos
        ProdutoSelecionado = null;
        Quantidade = "";
        NovoProdutoBebida = "";
        NovoProdutoTamanho = "";
        NovoProdutoMaterial = "";
        ValorUnitario = "0";
    }

    [RelayCommand]
    private void RemoverProduto(ProdutoSelecionadoItem item)
    {
        if (item != null)
        {
            ProdutosSelecionados.Remove(item);
            AtualizarValorTotal();
        }
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        CurrentView = viewName switch
        {
            "Inicio" => _inicioView,
            "Estoque" => _estoqueView,
            "Historico" => _historicoView,
            _ => CurrentView
        };
    }

    partial void OnTipoMovimentacaoChanged(string value)
    {
        IsSaida = value == "Saída";
        OnPropertyChanged(nameof(IsEntrada)); // Garante que IsEntrada seja atualizado

        if (!IsSaida)
        {
            TipoSaida = null;
            // Mostra todos os produtos no caso de entrada
            ProdutosFiltrados = new ObservableCollection<MainRepository.Produto>(Produtos);
        }
        else
        {
            // Filtra apenas produtos com estoque para saída
            var produtosComEstoque = Produtos.Where(p =>
            {
                var estoque = _estoqueViewModel.Estoque
                    .FirstOrDefault(e => e.ProductId == p.ProductId);
                return estoque?.Quantidade > 0;
            });
            ProdutosFiltrados = new ObservableCollection<MainRepository. Produto>(produtosComEstoque);
        }
        // Limpa a seleção atual ao trocar o tipo
        ProdutoSelecionado = null;
        NovoProdutoBebida = "";
        NovoProdutoTamanho = "";
        NovoProdutoMaterial = "";
        ValorUnitario = "0";
    }

    partial void OnProdutosSelecionadosChanged(ObservableCollection<ProdutoSelecionadoItem> value)
    {
        AtualizarValorTotal();
        value.CollectionChanged += ProdutosSelecionados_CollectionChanged;
    }

    private void ProdutosSelecionados_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        AtualizarValorTotal();
    }

    private void AtualizarValorTotal()
    {
        ValorTotalSelecionado = ProdutosSelecionados?
            .Where(p => p.TipoMovimentacao == "Saída")
            .Sum(p => p.ValorTotal) ?? 0m;
    }

    [RelayCommand]
    private void Limpar()
    {
        ProdutosSelecionados.Clear();
        ProdutoSelecionado = null;
        Quantidade = "";
        TipoSaida = null;
        TipoMovimentacao = "Entrada";
        ResponsavelSelecionado = null;
        
        AtualizarValorTotal();
    }

    [RelayCommand]
    private void RegistrarMovimentacao()
    {
        if (ProdutosSelecionados.Any() && !string.IsNullOrEmpty(ResponsavelSelecionado))
        {
            foreach (var item in ProdutosSelecionados)
            {
                var movimentacao = new MainRepository.Movimentacao
                {
                    Data = DateTime.Now,
                    Tipo = item.TipoMovimentacao,
                    ProductId = item.Produto.ProductId,
                    ProdutoDescricao = item.Produto.Descricao,
                    Quantidade = item.Quantidade,
                    Responsavel = ResponsavelSelecionado,
                    TipoSaida = item.TipoSaida,
                    ValorUnitario = item.ValorUnitario // <-- Garante que o valor unitário correto seja passado
                };

                _mainRepository.RegistrarMovimentacao(movimentacao);
            }

            _historicoViewModel.CarregarHistorico();
            _estoqueViewModel.CarregarEstoque();
            Limpar();
        }
    }

    [RelayCommand]
    public async Task SincronizarAsync()
    {
        try
        {
            await Task.Run(() => _databaseService.SincronizarEAtualizarBackupLocal());
            _estoqueViewModel.CarregarEstoque();
            _historicoViewModel.CarregarHistorico();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao sincronizar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void TrocarTema()
    {
        var currentTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault();
        var newTheme = currentTheme?.Source?.ToString().Contains("Dark.xaml") == true
            ? new Uri("Themes/Light.xaml", UriKind.Relative)
            : new Uri("Themes/Dark.xaml", UriKind.Relative);

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = newTheme });
    }

    public bool IsEntrada => !IsSaida;
}

public class ProdutoSelecionadoItem : ObservableObject
{
    public MainRepository.Produto Produto { get; set; } = null!;
    public string Descricao { get; set; } = "";
    
    private int quantidade;
    public int Quantidade 
    { 
        get => quantidade;
        set
        {
            if (SetProperty(ref quantidade, value))
                OnPropertyChanged(nameof(ValorTotal));
        }
    }
    
    public string TipoMovimentacao { get; set; } = "";
    public string? TipoSaida { get; set; }
    
    private decimal valorUnitario;
    public decimal ValorUnitario 
    { 
        get => valorUnitario;
        set
        {
            if (SetProperty(ref valorUnitario, value))
                OnPropertyChanged(nameof(ValorTotal));
        }
    }
    
    public decimal ValorTotal => Quantidade * ValorUnitario;
}
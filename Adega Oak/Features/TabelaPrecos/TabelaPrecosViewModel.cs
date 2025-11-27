using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Adega_Oak.Features.TabelaPrecos;

public partial class TabelaPrecosViewModel : ObservableObject
{
    private readonly EstoqueRepository _estoqueRepository;
    private readonly CollectionViewSource _produtosViewSource;
    private readonly ObservableCollection<ProdutoPrecoItem> _produtos;
    private const decimal MARGEM_MINIMA = 1.10m; // 10% de margem mínima

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private ProdutoPrecoItem? _produtoSelecionado;

    public ICollectionView ProdutosView => _produtosViewSource.View;
    public ObservableCollection<ProdutoPrecoItem> Produtos => _produtos;

    public TabelaPrecosViewModel(EstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
        _produtos = new ObservableCollection<ProdutoPrecoItem>();
        _produtosViewSource = new CollectionViewSource { Source = _produtos };
        _produtosViewSource.Filter += ProdutosViewSource_Filter;
    }

    private void ProdutosViewSource_Filter(object sender, FilterEventArgs e)
    {
        if (e.Item is not ProdutoPrecoItem produto || string.IsNullOrWhiteSpace(Filtro))
        {
            e.Accepted = true;
            return;
        }

        e.Accepted = produto.Descricao.Contains(Filtro, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se o preço está acima da margem mínima obrigatória
    /// </summary>
    private bool ValidarPrecoMinimo(decimal custo, decimal preco, decimal quantidade, string tipoPreco, out decimal precoMinimo)
    {
        precoMinimo = (custo * quantidade) * MARGEM_MINIMA;
        
        if (preco < precoMinimo)
        {
            System.Windows.MessageBox.Show(
                $"? PREÇO {tipoPreco.ToUpper()} INVÁLIDO\n\n" +
                $"Custo por unidade: R$ {custo:F2}\n" +
                $"Quantidade: {quantidade}\n" +
                $"Preço mínimo (custo total + 10%): R$ {precoMinimo:F2}\n" +
                $"Preço informado: R$ {preco:F2}\n\n" +
                $"O preço {tipoPreco} não pode ser menor que (custo × quantidade) + 10%.",
                $"Validação de Preço {tipoPreco}",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning
            );
            return false;
        }
        return true;
    }

    [RelayCommand]
    private void AtualizarPrecoVenda(ProdutoPrecoItem item)
    {
        if (item.ValorVenda <= 0)
            return;

        if (!ValidarPrecoMinimo(item.ValorCusto, item.ValorVenda, 1, "de venda", out var precoMinimo))
        {
            item.ValorVenda = precoMinimo;
            return;
        }

        _estoqueRepository.AtualizarPrecoVenda(item.Descricao, item.ValorVenda);
    }

    [RelayCommand]
    private void AtualizarCaixaPrecos(ProdutoPrecoItem item)
    {
        if (item.QuantidadeCaixa <= 0 || item.ValorCaixa <= 0)
            return;

        if (!ValidarPrecoMinimo(item.ValorCusto, item.ValorCaixa, item.QuantidadeCaixa, "de caixa", out var precoMinimo))
        {
            item.ValorCaixa = precoMinimo;
            return;
        }

        _estoqueRepository.AtualizarCaixaPreco(item.Descricao, item.QuantidadeCaixa, item.ValorCaixa);
    }

    [RelayCommand]
    private void AtualizarPrecoAtacado(ProdutoPrecoItem item)
    {
        if (item.ValorAtacadoCaixa <= 0)
            return;

        if (!ValidarPrecoMinimo(item.ValorCusto, item.ValorAtacadoCaixa, item.QuantidadeCaixa, "de atacado", out var precoMinimo))
        {
            item.ValorAtacadoCaixa = precoMinimo;
            return;
        }

        _estoqueRepository.AtualizarPrecoAtacado(item.Descricao, item.ValorAtacadoCaixa);
    }

    [RelayCommand]
    public void AtualizarQuantidadeMinimaAtacado(ProdutoPrecoItem item)
    {
        if (item.QuantidadeMinimaAtacado <= 0)
            item.QuantidadeMinimaAtacado = 20;

        if (item.ProductId > 0)
        {
            _estoqueRepository.AtualizarQuantidadeMinimaAtacado(item.ProductId, item.QuantidadeMinimaAtacado);
        }
    }

    private void CarregarTabela()
    {
        var estoque = _estoqueRepository.CarregarEstoqueCompleto();
        _produtos.Clear();

        foreach (var item in estoque)
        {
            var valorVenda = item.ValorVenda > 0 ? item.ValorVenda : item.Valor;
            var quantidadeCaixa = item.QuantidadeCaixa > 0 ? item.QuantidadeCaixa : 1;
            var valorCaixa = item.ValorCaixa > 0 ? item.ValorCaixa : (valorVenda * quantidadeCaixa);
            var valorAtacadoCaixa = item.ValorAtacadoCaixa > 0 ? item.ValorAtacadoCaixa : valorCaixa;
            var minimoAtacado = item.QuantidadeMinimaAtacado > 0 ? item.QuantidadeMinimaAtacado : 20;

            _produtos.Add(new ProdutoPrecoItem
            {
                ProductId = item.ProductId,
                Descricao = $"{item.Bebida} - {item.Tamanho} - {item.Material}",
                ValorCusto = item.Valor,
                ValorVenda = valorVenda,
                QuantidadeCaixa = quantidadeCaixa,
                ValorCaixa = valorCaixa,
                ValorAtacadoCaixa = valorAtacadoCaixa,
                QuantidadeMinimaAtacado = minimoAtacado
            });
        }
    }

    public void CarregarTabelaPrecos()
    {
        CarregarTabela();
    }

    partial void OnFiltroChanged(string value)
    {
        _produtosViewSource.View.Refresh();
    }
}

public class ProdutoPrecoItem : ObservableObject
{
    public int ProductId { get; set; } = 0;
    public string Descricao { get; set; } = string.Empty;

    private decimal _valorCusto;
    public decimal ValorCusto
    {
        get => _valorCusto;
        set
        {
            if (SetProperty(ref _valorCusto, value))
            {
                CalcularPorcentagensDeLucro();
                AtualizarValorCaixa();
            }
        }
    }

    private decimal _valorVenda;
    public decimal ValorVenda
    {
        get => _valorVenda;
        set
        {
            if (SetProperty(ref _valorVenda, value))
            {
                CalcularPorcentagensDeLucro();
                AtualizarValorCaixa();
            }
        }
    }

    private int _quantidadeCaixa = 1;
    public int QuantidadeCaixa
    {
        get => _quantidadeCaixa;
        set
        {
            if (SetProperty(ref _quantidadeCaixa, value))
            {
                AtualizarValorCaixa();
                CalcularPorcentagensDeLucro();
            }
        }
    }

    private decimal _valorCaixa;
    public decimal ValorCaixa
    {
        get => _valorCaixa;
        set
        {
            if (SetProperty(ref _valorCaixa, value))
            {
                CalcularPorcentagensDeLucro();
            }
        }
    }

    private decimal _valorAtacadoCaixa;
    public decimal ValorAtacadoCaixa
    {
        get => _valorAtacadoCaixa;
        set
        {
            if (SetProperty(ref _valorAtacadoCaixa, value))
            {
                CalcularPorcentagensDeLucro();
            }
        }
    }

    private int _quantidadeMinimaAtacado = 20;
    public int QuantidadeMinimaAtacado
    {
        get => _quantidadeMinimaAtacado;
        set => SetProperty(ref _quantidadeMinimaAtacado, value);
    }

    private decimal _porcentagemLucro;
    public decimal PorcentagemLucro
    {
        get => _porcentagemLucro;
        private set => SetProperty(ref _porcentagemLucro, value);
    }

    private decimal _porcentagemLucroCaixa;
    public decimal PorcentagemLucroCaixa
    {
        get => _porcentagemLucroCaixa;
        private set => SetProperty(ref _porcentagemLucroCaixa, value);
    }

    private decimal _porcentagemLucroAtacado;
    public decimal PorcentagemLucroAtacado
    {
        get => _porcentagemLucroAtacado;
        private set => SetProperty(ref _porcentagemLucroAtacado, value);
    }

    /// <summary>
    /// Calcula ambas as porcentagens de lucro em uma única passada
    /// </summary>
    private void CalcularPorcentagensDeLucro()
    {
        // Lucro unitário
        PorcentagemLucro = _valorCusto > 0 && _valorVenda > 0
            ? ((_valorVenda - _valorCusto) / _valorCusto) * 100
            : 0;

        // Lucro de caixa
        var custoTotalCaixa = _valorCusto * (_quantidadeCaixa > 0 ? _quantidadeCaixa : 1);
        PorcentagemLucroCaixa = custoTotalCaixa > 0 && _valorCaixa > 0
            ? ((_valorCaixa - custoTotalCaixa) / custoTotalCaixa) * 100
            : 0;

        // Lucro de atacado
        PorcentagemLucroAtacado = custoTotalCaixa > 0 && _valorAtacadoCaixa > 0
            ? ((_valorAtacadoCaixa - custoTotalCaixa) / custoTotalCaixa) * 100
            : 0;
    }

    private void AtualizarValorCaixa()
    {
        if (QuantidadeCaixa > 0)
        {
            ValorCaixa = ValorVenda * QuantidadeCaixa;
        }
    }
}

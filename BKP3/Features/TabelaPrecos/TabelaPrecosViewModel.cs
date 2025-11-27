using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;

namespace Adega_Oak.Features.TabelaPrecos;

public partial class TabelaPrecosViewModel : ObservableObject
{
    private readonly EstoqueRepository _estoqueRepository;
    private readonly CollectionViewSource _produtosViewSource;

    [ObservableProperty]
    private ObservableCollection<ProdutoPrecoItem> produtos = new();

    [ObservableProperty]
    private string filtro = string.Empty;

    public ICollectionView ProdutosView => _produtosViewSource.View;

    public TabelaPrecosViewModel(EstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
        _produtosViewSource = new CollectionViewSource { Source = Produtos };
        _produtosViewSource.Filter += (s, e) =>
        {
            if (e.Item is ProdutoPrecoItem item)
            {
                e.Accepted = string.IsNullOrWhiteSpace(Filtro) || item.Descricao.Contains(Filtro, System.StringComparison.CurrentCultureIgnoreCase);
            }
        };
        CarregarTabela();
    }

    public void CarregarTabela()
    {
        Produtos.Clear();
        var estoque = _estoqueRepository.CarregarEstoqueCompleto();
        foreach (var item in estoque.Where(e => e.Valor > 0))
        {
            // Calcula o preço de venda para que 30% do preço final seja lucro
            // Preço de venda = custo / (1 - 0,3)
            decimal precoVenda = item.Valor / (1 - 0.3m);
            Produtos.Add(new ProdutoPrecoItem
            {
                Descricao = $"{item.Bebida} - {item.Tamanho} - {item.Material}",
                ValorCusto = item.Valor,
                ValorVenda = decimal.Round(precoVenda, 2)
            });
        }
    }

    partial void OnFiltroChanged(string value)
    {
        _produtosViewSource.View.Refresh();
    }
}

public class ProdutoPrecoItem : ObservableObject
{
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCusto { get; set; }
    public decimal ValorVenda { get; set; }
}

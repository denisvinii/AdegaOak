namespace AdegaOak.Models.Models;

public class Produto
{
    public int Id { get; set; }
    public string Bebida { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public decimal Valor { get; set; }           // Custo
    public decimal ValorVenda { get; set; }      // Varejo unitário
    public int QuantidadeCaixa { get; set; } = 1;
    public decimal ValorCaixa { get; set; }      // Preço por caixa
    public decimal ValorAtacadoCaixa { get; set; } // Atacado por caixa
    public int EstoqueMinimo { get; set; } = 0;
    public int QuantidadeMinimaAtacado { get; set; } = 20;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public string Descricao => $"{Bebida} - {Tamanho} - {Material}";

    // Navigation
    public ICollection<Movimentacao> Movimentacoes { get; set; } = [];
    public ICollection<ComboComposicao> ComboComposicoes { get; set; } = [];
    public ICollection<Despesa> Despesas { get; set; } = [];
}

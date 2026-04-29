namespace AdegaOak.Models.Models;

public class Combo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoVenda { get; set; }
    public bool Ativo { get; set; } = true;
    public bool EhCopao { get; set; } = false; // false = Combo, true = Copão
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ComboComposicao> Composicao { get; set; } = [];
    public ICollection<ComboVenda> Vendas { get; set; } = [];
}

public class ComboComposicao
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = "ml";
    public bool DebitaEstoque { get; set; } = true;

    // Navigation
    public Combo? Combo { get; set; }
    public Produto? Produto { get; set; }
}

public class ComboVenda
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int UsuarioId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PrecoTotal { get; set; }
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    public string? Responsavel { get; set; }
    public string? Observacoes { get; set; }
    public string TipoMovimento { get; set; } = "Normal"; // "Normal" | "ValorZero"

    // Navigation
    public Combo? Combo { get; set; }
    public Usuario? Usuario { get; set; }
}

namespace AdegaOak.Models.Models;

public class Movimentacao
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = string.Empty;       // "Entrada" | "Saída"
    public string TipoVenda { get; set; } = "Varejo";      // "Varejo" | "Atacado"
    public int ProdutoId { get; set; }
    public string ProdutoDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int UsuarioId { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public string? TipoSaida { get; set; }                 // Consumo, Vencimento, Avaria, Combo, etc.
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal => ValorUnitario * Quantidade;

    // Navigation
    public Produto? Produto { get; set; }
    public Usuario? Usuario { get; set; }
}

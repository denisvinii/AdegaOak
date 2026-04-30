namespace AdegaOak.Models.Models;

public class Venda
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public int UsuarioId { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal ValorDinheiro { get; set; }
    public decimal ValorCartao { get; set; }
    public decimal ValorPix { get; set; }
    public string? Observacao { get; set; }

    // Navigation
    public Usuario? Usuario { get; set; }
    public List<Movimentacao> Movimentacoes { get; set; } = new();
}

namespace AdegaOak.Models.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "funcionario"; // "admin" | "funcionario"
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Movimentacao> Movimentacoes { get; set; } = [];
    public ICollection<ComboVenda> ComboVendas { get; set; } = [];
}

namespace AdegaOak.Models.Models;

public enum TipoDespesa
{
    Aluguel = 0,
    Salario = 1,
    Luz = 2,
    Agua = 3,
    Internet = 4,
    Manutencao = 5,
    Limpeza = 6,
    Publicidade = 7,
    Transporte = 8,
    Outros = 9,
    Consumo = 10,
    Vencimento = 11,
    Avaria = 12,
    ConsumoCopoes = 13
}

public class Despesa
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public TipoDespesa Tipo { get; set; }
    public bool Pago { get; set; } = false;
    public DateTime? DataPagamento { get; set; }
    public string? Notas { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // For product-linked expenses (Consumo, Vencimento, Avaria)
    public int? ProdutoId { get; set; }
    public int Quantidade { get; set; } = 0;

    // Navigation
    public Produto? Produto { get; set; }
}

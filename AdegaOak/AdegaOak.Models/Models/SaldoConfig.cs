namespace AdegaOak.Models.Models;

/// <summary>
/// Stores the manually-set capital values for balance calculation.
/// There is always exactly one row (Id = 1).
/// </summary>
public class SaldoConfig
{
    public int Id { get; set; } = 1;
    public decimal CapitalAdmin { get; set; } = 0;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}

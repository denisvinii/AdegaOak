using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IDashboardService
{
    Task<SaldoDto> GetSaldoAsync();
    Task<SaldoDto> UpdateCapitalAdminAsync(decimal valor);
    Task<DashboardDto> GetDashboardAsync(FiltrosDashboardRequest filtros);
    Task<RelatorioVendasDto> GetRelatorioVendasAsync(FiltrosRelatorioRequest filtros);
    Task<List<ProdutoMargemDto>> GetProdutosPorMargemAsync(string ordenacao = "menor");
}

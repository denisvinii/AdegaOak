using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("saldo")]
    public async Task<ActionResult<SaldoDto>> GetSaldo()
    {
        try
        {
            var saldo = await dashboardService.GetSaldoAsync();
            return Ok(saldo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in GetSaldo: {ex.Message}");
            return StatusCode(500, new { error = "Erro ao carregar saldo", details = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("saldo/capital-admin")]
    public async Task<ActionResult<SaldoDto>> UpdateCapitalAdmin([FromBody] decimal valor)
    {
        try
        {
            var saldo = await dashboardService.UpdateCapitalAdminAsync(valor);
            return Ok(saldo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in UpdateCapitalAdmin: {ex.Message}");
            return StatusCode(500, new { error = "Erro ao atualizar capital", details = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromBody] FiltrosDashboardRequest filtros)
    {
        try
        {
            var dashboard = await dashboardService.GetDashboardAsync(filtros);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in GetDashboard: {ex.Message}");
            Console.WriteLine($"[DASHBOARD] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[DASHBOARD] Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Erro ao carregar dashboard", details = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("relatorio-vendas")]
    public async Task<ActionResult<RelatorioVendasDto>> GetRelatorioVendas([FromBody] FiltrosRelatorioRequest filtros)
    {
        try
        {
            var relatorio = await dashboardService.GetRelatorioVendasAsync(filtros);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in GetRelatorioVendas: {ex.Message}");
            Console.WriteLine($"[DASHBOARD] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[DASHBOARD] Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Erro ao carregar relatório de vendas", details = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpGet("produtos-margem")]
    public async Task<ActionResult<List<ProdutoMargemDto>>> GetProdutosPorMargem([FromQuery] string ordenacao = "menor")
    {
        try
        {
            var produtos = await dashboardService.GetProdutosPorMargemAsync(ordenacao);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in GetProdutosPorMargem: {ex.Message}");
            Console.WriteLine($"[DASHBOARD] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = "Erro ao carregar produtos por margem", details = ex.Message });
        }
    }
}

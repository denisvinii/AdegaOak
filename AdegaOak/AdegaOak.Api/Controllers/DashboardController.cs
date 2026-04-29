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
        var saldo = await dashboardService.GetSaldoAsync();
        return Ok(saldo);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("saldo/capital-admin")]
    public async Task<ActionResult<SaldoDto>> UpdateCapitalAdmin([FromBody] decimal valor)
    {
        var saldo = await dashboardService.UpdateCapitalAdminAsync(valor);
        return Ok(saldo);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromBody] FiltrosDashboardRequest filtros)
    {
        var dashboard = await dashboardService.GetDashboardAsync(filtros);
        return Ok(dashboard);
    }
}

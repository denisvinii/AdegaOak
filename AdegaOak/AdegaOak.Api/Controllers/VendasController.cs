using System.Security.Claims;
using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendasController(IVendaService vendaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VendaDto>>> GetAll()
    {
        var vendas = await vendaService.GetAllAsync();
        return Ok(vendas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendaDto>> GetById(int id)
    {
        var venda = await vendaService.GetByIdAsync(id);
        return venda == null ? NotFound() : Ok(venda);
    }

    [HttpPost]
    public async Task<ActionResult<VendaDto>> Create([FromBody] CreateVendaRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var responsavel = User.FindFirstValue("nome") ?? User.Identity!.Name!;

            var venda = await vendaService.CreateAsync(request, usuarioId, responsavel);
            return CreatedAtAction(nameof(GetById), new { id = venda.Id }, venda);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<VendaResumoDto>> GetResumo([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        var resumo = await vendaService.GetResumoAsync(dataInicio, dataFim);
        return Ok(resumo);
    }

    [HttpGet("hoje")]
    public async Task<ActionResult<List<VendaDto>>> GetVendasHoje()
    {
        var hoje = DateTime.UtcNow.Date;
        var amanha = hoje.AddDays(1);
        var vendas = await vendaService.GetVendasPorPeriodoAsync(hoje, amanha);
        return Ok(vendas);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelarVenda(int id)
    {
        try
        {
            await vendaService.CancelarVendaAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using System.Security.Claims;
using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DespesasController(IDespesaService despesaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DespesaDto>>> GetAll()
    {
        var despesas = await despesaService.GetAllAsync();
        return Ok(despesas);
    }

    [HttpGet("periodo/{mes}/{ano}")]
    public async Task<ActionResult<List<DespesaDto>>> GetByPeriodo(int mes, int ano)
    {
        var despesas = await despesaService.GetByPeriodoAsync(mes, ano);
        return Ok(despesas);
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<DespesaResumoDto>> GetResumo([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resumo = await despesaService.GetResumoAsync(mes, ano);
        return Ok(resumo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DespesaDto>> GetById(int id)
    {
        var despesa = await despesaService.GetByIdAsync(id);
        return despesa == null ? NotFound() : Ok(despesa);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<DespesaDto>> Create([FromBody] CreateDespesaRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var responsavel = User.FindFirstValue("nome") ?? User.Identity!.Name!;

            var despesa = await despesaService.CreateAsync(request, usuarioId, responsavel);
            return CreatedAtAction(nameof(GetById), new { id = despesa.Id }, despesa);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<DespesaDto>> Update(int id, [FromBody] UpdateDespesaRequest request)
    {
        try
        {
            var despesa = await despesaService.UpdateAsync(id, request);
            return Ok(despesa);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id}/pagar")]
    public async Task<IActionResult> MarcarPaga(int id, [FromBody] bool pago)
    {
        try
        {
            await despesaService.MarcarPagaAsync(id, pago);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await despesaService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

using System.Security.Claims;
using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MovimentacoesController(IMovimentacaoService movimentacaoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovimentacaoDto>>> GetAll()
    {
        var movimentacoes = await movimentacaoService.GetAllAsync();
        return Ok(movimentacoes);
    }

    [HttpPost("filtrar")]
    public async Task<ActionResult<List<MovimentacaoDto>>> GetByFiltros([FromBody] MovimentacaoFiltroRequest filtros)
    {
        var movimentacoes = await movimentacaoService.GetByFiltrosAsync(filtros);
        return Ok(movimentacoes);
    }

    [HttpPost("resumo")]
    public async Task<ActionResult<MovimentacaoResumoDto>> GetResumo([FromBody] MovimentacaoFiltroRequest filtros)
    {
        var resumo = await movimentacaoService.GetResumoAsync(filtros);
        return Ok(resumo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovimentacaoDto>> GetById(int id)
    {
        var movimentacao = await movimentacaoService.GetByIdAsync(id);
        return movimentacao == null ? NotFound() : Ok(movimentacao);
    }

    [HttpPost]
    public async Task<ActionResult<MovimentacaoDto>> Create([FromBody] CreateMovimentacaoRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var responsavel = User.FindFirstValue("nome") ?? User.Identity!.Name!;

            var movimentacao = await movimentacaoService.CreateAsync(request, usuarioId, responsavel);
            return CreatedAtAction(nameof(GetById), new { id = movimentacao.Id }, movimentacao);
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
            await movimentacaoService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

using System.Security.Claims;
using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CombosController(IComboService comboService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ComboDto>>> GetAll([FromQuery] bool? ehCopao)
    {
        var combos = await comboService.GetAllAsync(ehCopao);
        return Ok(combos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComboDto>> GetById(int id)
    {
        var combo = await comboService.GetByIdAsync(id);
        return combo == null ? NotFound() : Ok(combo);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<ComboDto>> Create([FromBody] CreateComboRequest request)
    {
        try
        {
            var combo = await comboService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = combo.Id }, combo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ComboDto>> Update(int id, [FromBody] UpdateComboRequest request)
    {
        try
        {
            var combo = await comboService.UpdateAsync(id, request);
            return Ok(combo);
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

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await comboService.DeleteAsync(id);
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

    [HttpGet("vendas")]
    public async Task<ActionResult<List<ComboVendaDto>>> GetVendas(
        [FromQuery] int? comboId,
        [FromQuery] int? mes,
        [FromQuery] int? ano)
    {
        var vendas = await comboService.GetVendasAsync(comboId, mes, ano);
        return Ok(vendas);
    }

    [HttpPost("vendas")]
    public async Task<ActionResult<ComboVendaDto>> CreateVenda([FromBody] CreateComboVendaRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var responsavel = User.FindFirstValue("nome") ?? User.Identity!.Name!;

            var venda = await comboService.CreateVendaAsync(request, usuarioId, responsavel);
            return CreatedAtAction(nameof(GetVendas), new { comboId = venda.ComboId }, venda);
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

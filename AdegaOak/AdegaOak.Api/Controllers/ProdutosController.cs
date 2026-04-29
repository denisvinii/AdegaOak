using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProdutosController(IProdutoService produtoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProdutoDto>>> GetAll()
    {
        var produtos = await produtoService.GetAllAsync();
        return Ok(produtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> GetById(int id)
    {
        var produto = await produtoService.GetByIdAsync(id);
        return produto == null ? NotFound() : Ok(produto);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Create([FromBody] CreateProdutoRequest request)
    {
        var produto = await produtoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}/precos")]
    public async Task<ActionResult<ProdutoDto>> UpdatePrecos(int id, [FromBody] UpdatePrecosRequest request)
    {
        try
        {
            var produto = await produtoService.UpdatePrecosAsync(id, request);
            return Ok(produto);
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
            await produtoService.DeleteAsync(id);
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

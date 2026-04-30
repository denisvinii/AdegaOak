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
        try
        {
            var produto = await produtoService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno ao criar produto", details = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}/precos")]
    public async Task<ActionResult<ProdutoDto>> UpdatePrecos(int id, [FromBody] UpdatePrecosRequest request)
    {
        try
        {
            Console.WriteLine($"[PRODUTOS] Atualizando preços do produto {id}");
            Console.WriteLine($"[PRODUTOS] Valores recebidos: Custo={request.Valor}, Venda={request.ValorVenda}, Caixa={request.ValorCaixa}, Atacado={request.ValorAtacadoCaixa}");
            
            var produto = await produtoService.UpdatePrecosAsync(id, request);
            
            Console.WriteLine($"[PRODUTOS] Preços atualizados com sucesso para produto {id}");
            return Ok(produto);
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"[PRODUTOS] Produto {id} não encontrado: {ex.Message}");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[PRODUTOS] Validação falhou para produto {id}: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PRODUTOS] Erro inesperado ao atualizar produto {id}: {ex.Message}");
            Console.WriteLine($"[PRODUTOS] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Erro interno ao atualizar preços", details = ex.Message });
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

    [HttpGet("estoque")]
    public async Task<ActionResult<List<EstoqueProdutoDto>>> GetEstoque()
    {
        var estoque = await produtoService.GetEstoqueAsync();
        return Ok(estoque);
    }
}

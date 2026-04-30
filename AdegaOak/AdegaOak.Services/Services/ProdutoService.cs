using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AdegaOak.Services.Services;

public class ProdutoService(
    IProdutoRepository produtoRepository,
    IMemoryCache cache,
    ILogger<ProdutoService> logger) : IProdutoService
{
    private const string CacheProdutosKey = "produtos_ativos";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(3); // Cache de 3 minutos

    public async Task<List<ProdutoDto>> GetAllAsync()
    {
        return await cache.GetOrCreateAsync(CacheProdutosKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            entry.SetPriority(CacheItemPriority.Normal);
            
            var estoque = await produtoRepository.GetEstoqueComQuantidadeAsync();
            return estoque.Select(e => MapToDto(e.Produto, e.Quantidade)).ToList();
        }) ?? new List<ProdutoDto>();
    }

    public async Task<ProdutoDto?> GetByIdAsync(int id)
    {
        var produto = await produtoRepository.GetByIdAsync(id);
        if (produto == null) return null;

        var quantidade = await produtoRepository.GetQuantidadeAsync(id);
        return MapToDto(produto, quantidade);
    }

    public async Task<ProdutoDto> CreateAsync(CreateProdutoRequest request)
    {
        // Validação 1: Produto duplicado
        if (await produtoRepository.ProdutoExisteAsync(request.Bebida, request.Tamanho, request.Material))
        {
            throw new InvalidOperationException(
                $"Produto '{request.Bebida} - {request.Tamanho} - {request.Material}' já existe no sistema.");
        }

        // Validação 2: Quantidade de caixa deve ser >= 1
        if (request.QuantidadeCaixa < 1)
        {
            throw new InvalidOperationException("Quantidade de caixa deve ser no mínimo 1.");
        }

        // Validação 3: Valores devem ser positivos
        if (request.Valor <= 0)
        {
            throw new InvalidOperationException("Valor de custo deve ser maior que zero.");
        }

        if (request.ValorVenda <= 0)
        {
            throw new InvalidOperationException("Valor de venda deve ser maior que zero.");
        }

        if (request.ValorCaixa <= 0)
        {
            throw new InvalidOperationException("Valor da caixa deve ser maior que zero.");
        }

        if (request.ValorAtacadoCaixa <= 0)
        {
            throw new InvalidOperationException("Valor de atacado deve ser maior que zero.");
        }

        // Validação 4: Margem mínima de 10% sobre o custo
        var margemMinima = request.Valor * 1.10m;
        if (request.ValorVenda < margemMinima)
        {
            throw new InvalidOperationException(
                $"Valor de venda deve ser no mínimo R$ {margemMinima:F2} (10% acima do custo de R$ {request.Valor:F2}).");
        }

        // Validação 5: Valor da caixa deve ser maior que valor de venda unitário
        if (request.ValorCaixa <= request.ValorVenda)
        {
            throw new InvalidOperationException(
                $"Valor da caixa (R$ {request.ValorCaixa:F2}) deve ser maior que o valor de venda unitário (R$ {request.ValorVenda:F2}).");
        }

        // Validação 6: Valor de atacado deve ser maior que valor de venda unitário
        if (request.ValorAtacadoCaixa <= request.ValorVenda)
        {
            throw new InvalidOperationException(
                $"Valor de atacado (R$ {request.ValorAtacadoCaixa:F2}) deve ser maior que o valor de venda unitário (R$ {request.ValorVenda:F2}).");
        }

        // Validação 7: Valor de atacado deve ser menor que valor da caixa (desconto no atacado)
        if (request.ValorAtacadoCaixa >= request.ValorCaixa)
        {
            throw new InvalidOperationException(
                $"Valor de atacado (R$ {request.ValorAtacadoCaixa:F2}) deve ser menor que o valor da caixa (R$ {request.ValorCaixa:F2}) para oferecer desconto.");
        }

        // Validação 9: Estoque mínimo não pode ser negativo
        if (request.EstoqueMinimo < 0)
        {
            throw new InvalidOperationException("Estoque mínimo não pode ser negativo.");
        }

        var produto = new Produto
        {
            Bebida = request.Bebida.Trim(),
            Tamanho = request.Tamanho.Trim(),
            Material = request.Material.Trim(),
            Valor = request.Valor,
            ValorVenda = request.ValorVenda,
            QuantidadeCaixa = request.QuantidadeCaixa,
            ValorCaixa = request.ValorCaixa,
            ValorAtacadoCaixa = request.ValorAtacadoCaixa,
            EstoqueMinimo = request.EstoqueMinimo
        };

        await produtoRepository.CreateAsync(produto);
        
        // Invalidar cache após criar produto
        cache.Remove(CacheProdutosKey);
        
        return MapToDto(produto, 0);
    }

    public async Task<ProdutoDto> UpdatePrecosAsync(int id, UpdatePrecosRequest request)
    {
        var produto = await produtoRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");

        // Validação 1: Valores devem ser positivos
        if (request.Valor <= 0)
        {
            throw new InvalidOperationException("Valor de custo deve ser maior que zero.");
        }

        if (request.ValorVenda <= 0)
        {
            throw new InvalidOperationException("Valor de venda deve ser maior que zero.");
        }

        if (request.ValorCaixa <= 0)
        {
            throw new InvalidOperationException("Valor da caixa deve ser maior que zero.");
        }

        if (request.ValorAtacadoCaixa <= 0)
        {
            throw new InvalidOperationException("Valor de atacado deve ser maior que zero.");
        }

        // Validação 2: Margem mínima de 10% sobre o custo
        var margemMinima = request.Valor * 1.10m;
        if (request.ValorVenda < margemMinima)
        {
            throw new InvalidOperationException(
                $"Valor de venda deve ser no mínimo R$ {margemMinima:F2} (10% acima do custo de R$ {request.Valor:F2}).");
        }

        // Validação 3: Valor da caixa deve ser maior que valor de venda unitário
        if (request.ValorCaixa <= request.ValorVenda)
        {
            throw new InvalidOperationException(
                $"Valor da caixa (R$ {request.ValorCaixa:F2}) deve ser maior que o valor de venda unitário (R$ {request.ValorVenda:F2}).");
        }

        // Validação 4: Valor de atacado deve ser maior que valor de venda unitário
        if (request.ValorAtacadoCaixa <= request.ValorVenda)
        {
            throw new InvalidOperationException(
                $"Valor de atacado (R$ {request.ValorAtacadoCaixa:F2}) deve ser maior que o valor de venda unitário (R$ {request.ValorVenda:F2}).");
        }

        // Validação 5: Valor de atacado deve ser menor que valor da caixa (desconto no atacado)
        if (request.ValorAtacadoCaixa >= request.ValorCaixa)
        {
            throw new InvalidOperationException(
                $"Valor de atacado (R$ {request.ValorAtacadoCaixa:F2}) deve ser menor que o valor da caixa (R$ {request.ValorCaixa:F2}) para oferecer desconto.");
        }

        // Update values
        produto.Valor = request.Valor;
        produto.ValorVenda = request.ValorVenda;
        produto.ValorCaixa = request.ValorCaixa;
        produto.ValorAtacadoCaixa = request.ValorAtacadoCaixa;

        try
        {
            await produtoRepository.UpdateAsync(produto);
            
            // Invalidar cache após atualizar produto
            cache.Remove(CacheProdutosKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar produto {ProdutoId}", id);
            throw;
        }

        var quantidade = await produtoRepository.GetQuantidadeAsync(id);
        return MapToDto(produto, quantidade);
    }

    public async Task DeleteAsync(int id)
    {
        var quantidade = await produtoRepository.GetQuantidadeAsync(id);
        if (quantidade != 0)
            throw new InvalidOperationException("Não é possível excluir produto com estoque.");

        await produtoRepository.DeleteAsync(id);
        
        // Invalidar cache após deletar produto
        cache.Remove(CacheProdutosKey);
    }

    public async Task<List<EstoqueProdutoDto>> GetEstoqueAsync()
    {
        var estoque = await produtoRepository.GetEstoqueComQuantidadeAsync();
        return estoque.Select(e => new EstoqueProdutoDto(e.Produto.Id, e.Quantidade)).ToList();
    }

    private static ProdutoDto MapToDto(Produto p, int quantidade) =>
        new(
            p.Id,
            p.Bebida,
            p.Tamanho,
            p.Material,
            p.Descricao,
            p.Valor,
            p.ValorVenda,
            p.QuantidadeCaixa,
            p.ValorCaixa,
            p.ValorAtacadoCaixa,
            p.EstoqueMinimo,
            p.QuantidadeMinimaAtacado,
            quantidade,
            p.Valor * quantidade,
            quantidade <= p.EstoqueMinimo - 1,
            p.Ativo
        );
}

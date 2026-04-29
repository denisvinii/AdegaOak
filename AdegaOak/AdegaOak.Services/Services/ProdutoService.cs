using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;

namespace AdegaOak.Services.Services;

public class ProdutoService(IProdutoRepository produtoRepository) : IProdutoService
{
    public async Task<List<ProdutoDto>> GetAllAsync()
    {
        var estoque = await produtoRepository.GetEstoqueComQuantidadeAsync();
        return estoque.Select(e => MapToDto(e.Produto, e.Quantidade)).ToList();
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
        var produto = new Produto
        {
            Bebida = request.Bebida,
            Tamanho = request.Tamanho,
            Material = request.Material,
            Valor = request.Valor,
            ValorVenda = request.ValorVenda,
            QuantidadeCaixa = request.QuantidadeCaixa,
            ValorCaixa = request.ValorCaixa,
            ValorAtacadoCaixa = request.ValorAtacadoCaixa,
            EstoqueMinimo = request.EstoqueMinimo,
            QuantidadeMinimaAtacado = request.QuantidadeMinimaAtacado
        };

        await produtoRepository.CreateAsync(produto);
        return MapToDto(produto, 0);
    }

    public async Task<ProdutoDto> UpdatePrecosAsync(int id, UpdatePrecosRequest request)
    {
        var produto = await produtoRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");

        // Validate minimum margin (10%)
        var margemMinima = produto.Valor * 1.10m;
        if (request.ValorVenda < margemMinima)
            throw new InvalidOperationException($"Valor de venda deve ser no mínimo {margemMinima:C}.");

        produto.ValorVenda = request.ValorVenda;
        produto.ValorCaixa = request.ValorCaixa;
        produto.ValorAtacadoCaixa = request.ValorAtacadoCaixa;

        await produtoRepository.UpdateAsync(produto);
        var quantidade = await produtoRepository.GetQuantidadeAsync(id);
        return MapToDto(produto, quantidade);
    }

    public async Task DeleteAsync(int id)
    {
        var quantidade = await produtoRepository.GetQuantidadeAsync(id);
        if (quantidade != 0)
            throw new InvalidOperationException("Não é possível excluir produto com estoque.");

        await produtoRepository.DeleteAsync(id);
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
            quantidade <= p.EstoqueMinimo - 1
        );
}

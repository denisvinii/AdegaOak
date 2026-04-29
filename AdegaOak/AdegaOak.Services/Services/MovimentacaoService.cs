using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;

namespace AdegaOak.Services.Services;

public class MovimentacaoService(
    IMovimentacaoRepository movimentacaoRepository,
    IProdutoRepository produtoRepository) : IMovimentacaoService
{
    public async Task<List<MovimentacaoDto>> GetAllAsync()
    {
        var movimentacoes = await movimentacaoRepository.GetAllAsync();
        return movimentacoes.Select(MapToDto).ToList();
    }

    public async Task<MovimentacaoDto?> GetByIdAsync(int id)
    {
        var movimentacao = await movimentacaoRepository.GetByIdAsync(id);
        return movimentacao == null ? null : MapToDto(movimentacao);
    }

    public async Task<MovimentacaoDto> CreateAsync(CreateMovimentacaoRequest request, int usuarioId, string responsavel)
    {
        var produto = await produtoRepository.GetByIdAsync(request.ProdutoId)
            ?? throw new KeyNotFoundException($"Produto {request.ProdutoId} não encontrado.");

        var movimentacao = new Movimentacao
        {
            Data = DateTime.UtcNow,
            Tipo = request.Tipo,
            TipoVenda = request.TipoVenda,
            ProdutoId = request.ProdutoId,
            ProdutoDescricao = produto.Descricao,
            Quantidade = request.Quantidade,
            UsuarioId = usuarioId,
            Responsavel = responsavel,
            TipoSaida = request.TipoSaida,
            ValorUnitario = request.ValorUnitario
        };

        await movimentacaoRepository.CreateAsync(movimentacao);
        return MapToDto(movimentacao);
    }

    public async Task DeleteAsync(int id) =>
        await movimentacaoRepository.DeleteAsync(id);

    public async Task<List<MovimentacaoDto>> GetByFiltrosAsync(MovimentacaoFiltroRequest filtros)
    {
        var movimentacoes = await movimentacaoRepository.GetByFiltrosAsync(
            filtros.Tipo,
            filtros.TipoVenda,
            filtros.UsuarioId,
            filtros.DataInicio,
            filtros.DataFim,
            filtros.ProdutoId
        );
        return movimentacoes.Select(MapToDto).ToList();
    }

    public async Task<MovimentacaoResumoDto> GetResumoAsync(MovimentacaoFiltroRequest filtros)
    {
        var movimentacoes = await movimentacaoRepository.GetByFiltrosAsync(
            filtros.Tipo,
            filtros.TipoVenda,
            filtros.UsuarioId,
            filtros.DataInicio,
            filtros.DataFim,
            filtros.ProdutoId
        );

        var totalEntradas = movimentacoes
            .Where(m => m.Tipo == "Entrada")
            .Sum(m => m.ValorTotal);

        var totalSaidas = movimentacoes
            .Where(m => m.Tipo == "Saída")
            .Sum(m => m.ValorTotal);

        return new MovimentacaoResumoDto(
            totalEntradas,
            totalSaidas,
            totalSaidas - totalEntradas,
            movimentacoes.Count
        );
    }

    private static MovimentacaoDto MapToDto(Movimentacao m) =>
        new(
            m.Id,
            m.Data,
            m.Tipo,
            m.TipoVenda,
            m.ProdutoId,
            m.ProdutoDescricao,
            m.Quantidade,
            m.UsuarioId,
            m.Responsavel,
            m.TipoSaida,
            m.ValorUnitario,
            m.ValorTotal
        );
}

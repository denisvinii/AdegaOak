using AdegaOak.Data.Data;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdegaOak.Services.Services;

public class VendaService(AdegaOakDbContext db) : IVendaService
{
    public async Task<VendaDto> CreateAsync(CreateVendaRequest request, int usuarioId, string responsavel)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        
        try
        {
            // Validar que a soma das formas de pagamento bate com o total
            var valorTotalItens = request.Itens.Sum(i => i.Quantidade * i.ValorUnitario);
            var valorTotalPagamento = request.ValorDinheiro + request.ValorCartao + request.ValorPix;

            if (Math.Abs(valorTotalItens - valorTotalPagamento) > 0.01m)
            {
                throw new InvalidOperationException(
                    $"O valor total dos itens (R$ {valorTotalItens:F2}) não corresponde ao valor total do pagamento (R$ {valorTotalPagamento:F2})"
                );
            }

            // Criar a venda
            var venda = new Venda
            {
                Data = DateTime.UtcNow,
                UsuarioId = usuarioId,
                Responsavel = responsavel,
                ValorTotal = valorTotalItens,
                ValorDinheiro = request.ValorDinheiro,
                ValorCartao = request.ValorCartao,
                ValorPix = request.ValorPix,
                Observacao = request.Observacao
            };

            db.Vendas.Add(venda);
            await db.SaveChangesAsync();

            // Buscar todos os produtos de uma vez (otimização: evita N+1 queries)
            var produtoIds = request.Itens.Select(i => i.ProdutoId).Distinct().ToList();
            var produtos = await db.Produtos
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            // Validar que todos os produtos existem
            foreach (var produtoId in produtoIds)
            {
                if (!produtos.ContainsKey(produtoId))
                {
                    throw new KeyNotFoundException($"Produto com ID {produtoId} não encontrado");
                }
            }

            // Criar as movimentações para cada item
            var movimentacoes = request.Itens.Select(item => new Movimentacao
            {
                Data = venda.Data,
                Tipo = "Saída",
                TipoVenda = item.TipoVenda,
                ProdutoId = item.ProdutoId,
                ProdutoDescricao = produtos[item.ProdutoId].Descricao,
                Quantidade = item.Quantidade,
                UsuarioId = usuarioId,
                Responsavel = responsavel,
                ValorUnitario = item.ValorUnitario,
                VendaId = venda.Id
            }).ToList();

            db.Movimentacoes.AddRange(movimentacoes);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            // Retornar o DTO (usando o dicionário de produtos já carregado)
            return new VendaDto(
                venda.Id,
                venda.Data,
                venda.UsuarioId,
                venda.Responsavel,
                venda.ValorTotal,
                venda.ValorDinheiro,
                venda.ValorCartao,
                venda.ValorPix,
                venda.Observacao,
                request.Itens.Select(i => new ItemVendaDto(
                    i.ProdutoId,
                    produtos[i.ProdutoId].Descricao,
                    i.Quantidade,
                    i.ValorUnitario,
                    i.Quantidade * i.ValorUnitario,
                    i.TipoVenda
                )).ToList()
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<VendaDto>> GetAllAsync()
    {
        var vendas = await db.Vendas
            .AsNoTracking()
            .Include(v => v.Movimentacoes)
            .OrderByDescending(v => v.Data)
            .ToListAsync();

        return vendas.Select(v => new VendaDto(
            v.Id,
            v.Data,
            v.UsuarioId,
            v.Responsavel,
            v.ValorTotal,
            v.ValorDinheiro,
            v.ValorCartao,
            v.ValorPix,
            v.Observacao,
            v.Movimentacoes.Select(m => new ItemVendaDto(
                m.ProdutoId,
                m.ProdutoDescricao,
                m.Quantidade,
                m.ValorUnitario,
                m.ValorUnitario * m.Quantidade,
                m.TipoVenda
            )).ToList()
        )).ToList();
    }

    public async Task<VendaDto?> GetByIdAsync(int id)
    {
        var venda = await db.Vendas
            .AsNoTracking()
            .Include(v => v.Movimentacoes)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda == null) return null;

        return new VendaDto(
            venda.Id,
            venda.Data,
            venda.UsuarioId,
            venda.Responsavel,
            venda.ValorTotal,
            venda.ValorDinheiro,
            venda.ValorCartao,
            venda.ValorPix,
            venda.Observacao,
            venda.Movimentacoes.Select(m => new ItemVendaDto(
                m.ProdutoId,
                m.ProdutoDescricao,
                m.Quantidade,
                m.ValorUnitario,
                m.ValorUnitario * m.Quantidade,
                m.TipoVenda
            )).ToList()
        );
    }

    public async Task<VendaResumoDto> GetResumoAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var inicio = dataInicio ?? DateTime.UtcNow.AddMonths(-1);
        var fim = dataFim ?? DateTime.UtcNow;

        var vendas = await db.Vendas
            .AsNoTracking()
            .Where(v => v.Data >= inicio && v.Data <= fim)
            .ToListAsync();

        var totalVendas = vendas.Count;
        var valorTotalVendas = vendas.Sum(v => (double)v.ValorTotal);
        var totalDinheiro = vendas.Sum(v => (double)v.ValorDinheiro);
        var totalCartao = vendas.Sum(v => (double)v.ValorCartao);
        var totalPix = vendas.Sum(v => (double)v.ValorPix);

        var formasPagamento = new List<FormaPagamentoResumoDto>
        {
            new("Dinheiro", (decimal)totalDinheiro, vendas.Count(v => v.ValorDinheiro > 0), 
                valorTotalVendas > 0 ? (decimal)(totalDinheiro / valorTotalVendas * 100) : 0),
            new("Cartão", (decimal)totalCartao, vendas.Count(v => v.ValorCartao > 0), 
                valorTotalVendas > 0 ? (decimal)(totalCartao / valorTotalVendas * 100) : 0),
            new("PIX", (decimal)totalPix, vendas.Count(v => v.ValorPix > 0), 
                valorTotalVendas > 0 ? (decimal)(totalPix / valorTotalVendas * 100) : 0)
        };

        return new VendaResumoDto(
            totalVendas,
            (decimal)valorTotalVendas,
            (decimal)totalDinheiro,
            (decimal)totalCartao,
            (decimal)totalPix,
            formasPagamento
        );
    }

    public async Task<List<VendaDto>> GetVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var vendas = await db.Vendas
            .AsNoTracking()
            .Include(v => v.Movimentacoes)
            .Where(v => v.Data >= dataInicio && v.Data < dataFim)
            .OrderByDescending(v => v.Data)
            .ToListAsync();

        return vendas.Select(v => new VendaDto(
            v.Id,
            v.Data,
            v.UsuarioId,
            v.Responsavel,
            v.ValorTotal,
            v.ValorDinheiro,
            v.ValorCartao,
            v.ValorPix,
            v.Observacao,
            v.Movimentacoes.Select(m => new ItemVendaDto(
                m.ProdutoId,
                m.ProdutoDescricao,
                m.Quantidade,
                m.ValorUnitario,
                m.ValorUnitario * m.Quantidade,
                m.TipoVenda
            )).ToList()
        )).ToList();
    }

    public async Task CancelarVendaAsync(int id)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        
        try
        {
            var venda = await db.Vendas
                .Include(v => v.Movimentacoes)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                throw new KeyNotFoundException($"Venda com ID {id} não encontrada");
            }

            // Estornar as movimentações (criar movimentações de entrada para reverter as saídas)
            var movimentacoesEstorno = new List<Movimentacao>();
            
            foreach (var movimentacao in venda.Movimentacoes)
            {
                var estorno = new Movimentacao
                {
                    Data = DateTime.UtcNow,
                    Tipo = "Entrada",
                    TipoVenda = movimentacao.TipoVenda,
                    ProdutoId = movimentacao.ProdutoId,
                    ProdutoDescricao = movimentacao.ProdutoDescricao,
                    Quantidade = movimentacao.Quantidade,
                    UsuarioId = movimentacao.UsuarioId,
                    Responsavel = $"CANCELAMENTO - {movimentacao.Responsavel}",
                    ValorUnitario = movimentacao.ValorUnitario,
                    VendaId = null // Estorno não está vinculado à venda
                };

                movimentacoesEstorno.Add(estorno);
            }

            db.Movimentacoes.AddRange(movimentacoesEstorno);

            // Remover a venda e suas movimentações originais
            db.Movimentacoes.RemoveRange(venda.Movimentacoes);
            db.Vendas.Remove(venda);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

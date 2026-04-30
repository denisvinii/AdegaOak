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

            // Criar as movimentações para cada item
            var movimentacoes = new List<Movimentacao>();
            
            foreach (var item in request.Itens)
            {
                var produto = await db.Produtos.FindAsync(item.ProdutoId);
                if (produto == null)
                {
                    throw new KeyNotFoundException($"Produto com ID {item.ProdutoId} não encontrado");
                }

                var movimentacao = new Movimentacao
                {
                    Data = venda.Data,
                    Tipo = "Saída",
                    TipoVenda = item.TipoVenda,
                    ProdutoId = item.ProdutoId,
                    ProdutoDescricao = produto.Descricao,
                    Quantidade = item.Quantidade,
                    UsuarioId = usuarioId,
                    Responsavel = responsavel,
                    ValorUnitario = item.ValorUnitario,
                    VendaId = venda.Id
                };

                movimentacoes.Add(movimentacao);
            }

            db.Movimentacoes.AddRange(movimentacoes);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            // Retornar o DTO
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
                    db.Produtos.Find(i.ProdutoId)?.Descricao ?? "",
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
}

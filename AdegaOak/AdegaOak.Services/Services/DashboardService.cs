using AdegaOak.Data.Data;
using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdegaOak.Services.Services;

public class DashboardService(
    AdegaOakDbContext db,
    ISaldoRepository saldoRepository,
    IProdutoRepository produtoRepository) : IDashboardService
{
    public async Task<SaldoDto> GetSaldoAsync()
    {
        var configTask = saldoRepository.GetConfigAsync();

        // Execute all queries in parallel for better performance
        var movimentacoesEntradaTask = db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Entrada")
            .Select(m => (double)(m.ValorUnitario * m.Quantidade))
            .ToListAsync();

        var movimentacoesSaidaTask = db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída")
            .Select(m => (double)(m.ValorUnitario * m.Quantidade))
            .ToListAsync();

        var despesasPagasTask = db.Despesas
            .AsNoTracking()
            .Where(d => d.Pago)
            .Select(d => (double)d.Valor)
            .ToListAsync();

        var comboVendasTask = db.ComboVendas
            .AsNoTracking()
            .Select(cv => (double)cv.PrecoTotal)
            .ToListAsync();

        // Wait for all queries to complete
        await Task.WhenAll(configTask, movimentacoesEntradaTask, movimentacoesSaidaTask, despesasPagasTask, comboVendasTask);

        var config = await configTask;
        var movimentacoesEntrada = await movimentacoesEntradaTask;
        var movimentacoesSaida = await movimentacoesSaidaTask;
        var despesasPagas = await despesasPagasTask;
        var comboVendas = await comboVendasTask;

        var totalEntradas = movimentacoesEntrada.Any() ? movimentacoesEntrada.Sum() : 0.0;
        var totalSaidas = movimentacoesSaida.Any() ? movimentacoesSaida.Sum() : 0.0;
        var totalDespesasPagas = despesasPagas.Any() ? despesasPagas.Sum() : 0.0;
        var totalComboVendas = comboVendas.Any() ? comboVendas.Sum() : 0.0;

        var capitalEmpresa = (decimal)((totalSaidas - totalEntradas) + (double)config.CapitalAdmin - totalDespesasPagas + totalComboVendas);
        var saldo = (decimal)((totalSaidas - totalEntradas) - totalDespesasPagas + totalComboVendas);

        return new SaldoDto(
            capitalEmpresa,
            config.CapitalAdmin,
            saldo,
            (decimal)totalEntradas,
            (decimal)totalSaidas,
            (decimal)totalDespesasPagas,
            (decimal)totalComboVendas
        );
    }

    public async Task<SaldoDto> UpdateCapitalAdminAsync(decimal valor)
    {
        await saldoRepository.UpdateCapitalAdminAsync(valor);
        return await GetSaldoAsync();
    }

    public async Task<DashboardDto> GetDashboardAsync(FiltrosDashboardRequest filtros)
    {
        var dataInicio = filtros.DataInicio ?? DateTime.UtcNow.AddMonths(-1);
        var dataFim = filtros.DataFim ?? DateTime.UtcNow;
        var mesAtual = DateTime.UtcNow;

        // Parallel execution for ALL independent queries to improve performance
        var movimentacoesSaidaTask = db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída" && m.Data >= dataInicio && m.Data <= dataFim)
            .ToListAsync();

        var despesasTask = db.Despesas
            .AsNoTracking()
            .Where(d => d.Data >= dataInicio && d.Data <= dataFim)
            .ToListAsync();

        var estoqueTask = produtoRepository.GetEstoqueComQuantidadeAsync();

        var receitaMesTask = db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída" && m.Data.Month == mesAtual.Month && m.Data.Year == mesAtual.Year)
            .Select(m => (double)(m.ValorUnitario * m.Quantidade))
            .ToListAsync();

        var despesasMesTask = db.Despesas
            .AsNoTracking()
            .Where(d => d.Data.Month == mesAtual.Month && d.Data.Year == mesAtual.Year && d.Pago)
            .Select(d => (double)d.Valor)
            .ToListAsync();

        var totalMovimentacoesMesTask = db.Movimentacoes
            .AsNoTracking()
            .CountAsync(m => m.Data.Month == mesAtual.Month && m.Data.Year == mesAtual.Year);

        var saldoTask = GetSaldoAsync();

        // Wait for all parallel tasks
        await Task.WhenAll(
            movimentacoesSaidaTask,
            despesasTask,
            estoqueTask,
            receitaMesTask,
            despesasMesTask,
            totalMovimentacoesMesTask,
            saldoTask
        );

        var movimentacoesSaida = await movimentacoesSaidaTask;
        var despesas = await despesasTask;
        var estoque = await estoqueTask;
        var receitaMesList = await receitaMesTask;
        var despesasMesList = await despesasMesTask;
        
        var receitaMes = receitaMesList.Any() ? receitaMesList.Sum() : 0.0;
        var despesasMes = despesasMesList.Any() ? despesasMesList.Sum() : 0.0;
        var totalMovimentacoesMes = await totalMovimentacoesMesTask;

        // Vendas por dia
        var vendasPorDia = movimentacoesSaida
            .GroupBy(m => m.Data.Date)
            .Select(g => new VendasPorDiaDto(
                g.Key, 
                (decimal)g.Sum(m => (double)(m.ValorUnitario * m.Quantidade)), 
                g.Sum(m => m.Quantidade)))
            .OrderBy(v => v.Data)
            .ToList();

        // Top produtos
        var topProdutos = movimentacoesSaida
            .GroupBy(m => new { m.ProdutoId, m.ProdutoDescricao })
            .Select(g => new TopProdutoDto(
                g.Key.ProdutoId,
                g.Key.ProdutoDescricao,
                g.Sum(m => m.Quantidade),
                (decimal)g.Sum(m => (double)(m.ValorUnitario * m.Quantidade))
            ))
            .OrderByDescending(p => p.ValorTotal)
            .Take(10)
            .ToList();

        // Estoque baixo
        var estoqueBaixo = estoque
            .Where(e => e.Quantidade <= e.Produto.EstoqueMinimo)
            .Select(e => new EstoqueBaixoDto(
                e.Produto.Id,
                e.Produto.Descricao,
                e.Quantidade,
                e.Produto.EstoqueMinimo
            ))
            .ToList();

        // Despesas por tipo
        var despesasPorTipo = despesas
            .GroupBy(d => d.Tipo)
            .Select(g => new DespesasPorTipoDto(
                g.Key.ToString(),
                (decimal)g.Sum(d => (double)d.Valor),
                g.Count()
            ))
            .ToList();

        // Vendas por usuário
        var vendasPorUsuario = movimentacoesSaida
            .GroupBy(m => new { m.UsuarioId, m.Responsavel })
            .Select(g => new VendasPorUsuarioDto(
                g.Key.UsuarioId,
                g.Key.Responsavel,
                (decimal)g.Sum(m => (double)(m.ValorUnitario * m.Quantidade)),
                g.Sum(m => m.Quantidade)
            ))
            .OrderByDescending(v => v.Total)
            .ToList();

        return new DashboardDto(
            await saldoTask,
            vendasPorDia,
            topProdutos,
            estoqueBaixo,
            despesasPorTipo,
            vendasPorUsuario,
            (decimal)receitaMes,
            (decimal)despesasMes,
            totalMovimentacoesMes
        );
    }
}

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
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Error in GetSaldoAsync: {ex.Message}");
            Console.WriteLine($"[DASHBOARD] Stack trace: {ex.StackTrace}");
            
            // Return default values if error occurs
            return new SaldoDto(0, 0, 0, 0, 0, 0, 0);
        }
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

        // Execute queries sequentially to avoid DbContext concurrency issues
        var movimentacoesSaida = await db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída" && m.Data >= dataInicio && m.Data <= dataFim)
            .ToListAsync();

        var despesas = await db.Despesas
            .AsNoTracking()
            .Where(d => d.Data >= dataInicio && d.Data <= dataFim)
            .ToListAsync();

        var estoque = await produtoRepository.GetEstoqueComQuantidadeAsync();

        var receitaMesList = await db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída" && m.Data.Month == mesAtual.Month && m.Data.Year == mesAtual.Year)
            .Select(m => (double)(m.ValorUnitario * m.Quantidade))
            .ToListAsync();

        var despesasMesList = await db.Despesas
            .AsNoTracking()
            .Where(d => d.Data.Month == mesAtual.Month && d.Data.Year == mesAtual.Year && d.Pago)
            .Select(d => (double)d.Valor)
            .ToListAsync();

        var totalMovimentacoesMes = await db.Movimentacoes
            .AsNoTracking()
            .CountAsync(m => m.Data.Month == mesAtual.Month && m.Data.Year == mesAtual.Year);

        var saldo = await GetSaldoAsync();
        
        var receitaMes = receitaMesList.Any() ? receitaMesList.Sum() : 0.0;
        var despesasMes = despesasMesList.Any() ? despesasMesList.Sum() : 0.0;

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
            saldo,
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

    public async Task<RelatorioVendasDto> GetRelatorioVendasAsync(FiltrosRelatorioRequest filtros)
    {
        var dataInicio = DateTime.SpecifyKind(new DateTime(filtros.Ano, filtros.Mes ?? 1, 1), DateTimeKind.Utc);
        var dataFim = filtros.Mes.HasValue 
            ? DateTime.SpecifyKind(new DateTime(filtros.Ano, filtros.Mes.Value, DateTime.DaysInMonth(filtros.Ano, filtros.Mes.Value), 23, 59, 59), DateTimeKind.Utc)
            : DateTime.SpecifyKind(new DateTime(filtros.Ano, 12, 31, 23, 59, 59), DateTimeKind.Utc);

        // Query base de vendas
        var query = db.Movimentacoes
            .AsNoTracking()
            .Where(m => m.Tipo == "Saída" && m.Data >= dataInicio && m.Data <= dataFim);

        // Filtrar por usuário se especificado
        if (filtros.UsuarioId.HasValue)
        {
            query = query.Where(m => m.UsuarioId == filtros.UsuarioId.Value);
        }

        var movimentacoes = await query.ToListAsync();

        // Incluir vendas de combos
        var queryCombo = db.ComboVendas
            .AsNoTracking()
            .Include(cv => cv.Usuario)
            .Where(cv => cv.DataVenda >= dataInicio && cv.DataVenda <= dataFim);

        if (filtros.UsuarioId.HasValue)
        {
            queryCombo = queryCombo.Where(cv => cv.UsuarioId == filtros.UsuarioId.Value);
        }

        var comboVendas = await queryCombo.ToListAsync();

        // Agrupar vendas por mês
        var vendasPorMes = movimentacoes
            .GroupBy(m => new { m.Data.Year, m.Data.Month })
            .Select(g => new
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                TotalVendas = (decimal)g.Sum(m => (double)(m.ValorUnitario * m.Quantidade)),
                QuantidadeVendas = g.Sum(m => m.Quantidade)
            })
            .ToList();

        // Adicionar vendas de combos ao agrupamento mensal
        var combosPorMes = comboVendas
            .GroupBy(cv => new { cv.DataVenda.Year, cv.DataVenda.Month })
            .Select(g => new
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                TotalVendas = (decimal)g.Sum(cv => (double)cv.PrecoTotal),
                QuantidadeVendas = g.Sum(cv => cv.Quantidade)
            })
            .ToList();

        // Combinar vendas de produtos e combos
        var vendasCombinadas = vendasPorMes
            .Concat(combosPorMes)
            .GroupBy(v => new { v.Ano, v.Mes })
            .Select(g => new VendaMensalDto(
                g.Key.Mes,
                g.Key.Ano,
                ObterNomeMes(g.Key.Mes),
                g.Sum(v => v.TotalVendas),
                g.Sum(v => v.QuantidadeVendas),
                g.Sum(v => v.QuantidadeVendas) > 0 
                    ? g.Sum(v => v.TotalVendas) / g.Sum(v => v.QuantidadeVendas) 
                    : 0
            ))
            .OrderBy(v => v.Ano)
            .ThenBy(v => v.Mes)
            .ToList();

        // Vendas por usuário
        var vendasPorUsuario = movimentacoes
            .GroupBy(m => new { m.UsuarioId, m.Responsavel })
            .Select(g => new
            {
                UsuarioId = g.Key.UsuarioId,
                Nome = g.Key.Responsavel,
                Total = (decimal)g.Sum(m => (double)(m.ValorUnitario * m.Quantidade)),
                Quantidade = g.Sum(m => m.Quantidade)
            })
            .ToList();

        // Adicionar vendas de combos por usuário
        var combosPorUsuario = comboVendas
            .Where(cv => cv.Usuario != null)
            .GroupBy(cv => new { cv.UsuarioId, cv.Usuario.Nome })
            .Select(g => new
            {
                UsuarioId = g.Key.UsuarioId,
                Nome = g.Key.Nome,
                Total = (decimal)g.Sum(cv => (double)cv.PrecoTotal),
                Quantidade = g.Sum(cv => cv.Quantidade)
            })
            .ToList();

        var vendasUsuarioCombinadas = vendasPorUsuario
            .Concat(combosPorUsuario)
            .GroupBy(v => new { v.UsuarioId, v.Nome })
            .Select(g => new VendasPorUsuarioDto(
                g.Key.UsuarioId,
                g.Key.Nome,
                g.Sum(v => v.Total),
                g.Sum(v => v.Quantidade)
            ))
            .OrderByDescending(v => v.Total)
            .ToList();

        // Totais gerais
        var totalGeral = vendasCombinadas.Sum(v => v.TotalVendas);
        var quantidadeGeral = vendasCombinadas.Sum(v => v.QuantidadeVendas);
        var ticketMedioGeral = quantidadeGeral > 0 ? totalGeral / quantidadeGeral : 0;

        return new RelatorioVendasDto(
            vendasCombinadas,
            vendasUsuarioCombinadas,
            totalGeral,
            quantidadeGeral,
            ticketMedioGeral
        );
    }

    private static string ObterNomeMes(int mes)
    {
        return mes switch
        {
            1 => "Janeiro",
            2 => "Fevereiro",
            3 => "Março",
            4 => "Abril",
            5 => "Maio",
            6 => "Junho",
            7 => "Julho",
            8 => "Agosto",
            9 => "Setembro",
            10 => "Outubro",
            11 => "Novembro",
            12 => "Dezembro",
            _ => "Desconhecido"
        };
    }
}

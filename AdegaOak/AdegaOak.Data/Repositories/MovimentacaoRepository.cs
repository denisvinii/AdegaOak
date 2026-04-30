using AdegaOak.Models.Models;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface IMovimentacaoRepository
{
    Task<List<Movimentacao>> GetAllAsync();
    Task<Movimentacao?> GetByIdAsync(int id);
    Task<Movimentacao> CreateAsync(Movimentacao movimentacao);
    Task DeleteAsync(int id);
    Task<List<Movimentacao>> GetByFiltrosAsync(string? tipo, string? tipoVenda, int? usuarioId, DateTime? dataInicio, DateTime? dataFim, int? produtoId);
}

public class MovimentacaoRepository(AdegaOakDbContext db) : IMovimentacaoRepository
{
    public async Task<List<Movimentacao>> GetAllAsync() =>
        await db.Movimentacoes
            .AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Data)
            .Take(1000) // Limit to last 1000 records for performance
            .ToListAsync();

    public async Task<Movimentacao?> GetByIdAsync(int id) =>
        await db.Movimentacoes
            .AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Movimentacao> CreateAsync(Movimentacao movimentacao)
    {
        db.Movimentacoes.Add(movimentacao);
        await db.SaveChangesAsync();
        return movimentacao;
    }

    public async Task DeleteAsync(int id)
    {
        var movimentacao = await db.Movimentacoes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Movimentação {id} não encontrada.");
        db.Movimentacoes.Remove(movimentacao);
        await db.SaveChangesAsync();
    }

    public async Task<List<Movimentacao>> GetByFiltrosAsync(
        string? tipo, 
        string? tipoVenda, 
        int? usuarioId, 
        DateTime? dataInicio, 
        DateTime? dataFim, 
        int? produtoId)
    {
        var query = db.Movimentacoes
            .AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(m => m.Tipo == tipo);

        if (!string.IsNullOrEmpty(tipoVenda))
            query = query.Where(m => m.TipoVenda == tipoVenda);

        if (usuarioId.HasValue)
            query = query.Where(m => m.UsuarioId == usuarioId.Value);

        if (dataInicio.HasValue)
            query = query.Where(m => m.Data >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(m => m.Data <= dataFim.Value);

        if (produtoId.HasValue)
            query = query.Where(m => m.ProdutoId == produtoId.Value);

        return await query
            .OrderByDescending(m => m.Data)
            .Take(1000) // Limit to 1000 records for performance
            .ToListAsync();
    }
}

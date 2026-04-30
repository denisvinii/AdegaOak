using AdegaOak.Models.Models;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface IComboRepository
{
    Task<List<Combo>> GetAllAsync(bool? ehCopao = null);
    Task<Combo?> GetByIdAsync(int id);
    Task<Combo> CreateAsync(Combo combo);
    Task<Combo> UpdateAsync(Combo combo);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NomeExisteAsync(string nome, int? excludeId = null);
    Task<bool> FoiVendidoAsync(int comboId);
    Task<List<ComboVenda>> GetVendasAsync(int? comboId = null, int? mes = null, int? ano = null);
    Task<ComboVenda> CreateVendaAsync(ComboVenda venda);
}

public class ComboRepository(AdegaOakDbContext db) : IComboRepository
{
    public async Task<List<Combo>> GetAllAsync(bool? ehCopao = null)
    {
        var query = db.Combos
            .AsNoTracking()
            .Include(c => c.Composicao)
                .ThenInclude(cc => cc.Produto)
            .Where(c => c.Ativo)
            .AsQueryable();

        if (ehCopao.HasValue)
            query = query.Where(c => c.EhCopao == ehCopao.Value);

        return await query.OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<Combo?> GetByIdAsync(int id) =>
        await db.Combos
            .AsNoTracking()
            .Include(c => c.Composicao)
                .ThenInclude(cc => cc.Produto)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Combo> CreateAsync(Combo combo)
    {
        db.Combos.Add(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    public async Task<Combo> UpdateAsync(Combo combo)
    {
        db.Combos.Update(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    public async Task DeleteAsync(int id)
    {
        var combo = await db.Combos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Combo {id} não encontrado.");
        combo.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) =>
        await db.Combos.AnyAsync(c => c.Id == id && c.Ativo);

    public async Task<bool> NomeExisteAsync(string nome, int? excludeId = null)
    {
        var query = db.Combos.Where(c => c.Nome == nome && c.Ativo);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> FoiVendidoAsync(int comboId) =>
        await db.ComboVendas.AnyAsync(cv => cv.ComboId == comboId);

    public async Task<List<ComboVenda>> GetVendasAsync(int? comboId = null, int? mes = null, int? ano = null)
    {
        var query = db.ComboVendas
            .AsNoTracking()
            .AsQueryable();

        if (comboId.HasValue)
            query = query.Where(cv => cv.ComboId == comboId.Value);

        if (mes.HasValue)
            query = query.Where(cv => cv.DataVenda.Month == mes.Value);

        if (ano.HasValue)
            query = query.Where(cv => cv.DataVenda.Year == ano.Value);

        return await query
            .OrderByDescending(cv => cv.DataVenda)
            .Take(1000) // Limit to 1000 records for performance
            .ToListAsync();
    }

    public async Task<ComboVenda> CreateVendaAsync(ComboVenda venda)
    {
        db.ComboVendas.Add(venda);
        await db.SaveChangesAsync();
        return venda;
    }
}

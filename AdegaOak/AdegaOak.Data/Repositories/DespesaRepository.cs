using AdegaOak.Models.Models;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface IDespesaRepository
{
    Task<List<Despesa>> GetAllAsync();
    Task<Despesa?> GetByIdAsync(int id);
    Task<Despesa> CreateAsync(Despesa despesa);
    Task<Despesa> UpdateAsync(Despesa despesa);
    Task DeleteAsync(int id);
    Task<List<Despesa>> GetByPeriodoAsync(int mes, int ano);
    Task MarcarPagaAsync(int id, bool pago);
}

public class DespesaRepository(AdegaOakDbContext db) : IDespesaRepository
{
    public async Task<List<Despesa>> GetAllAsync() =>
        await db.Despesas
            .Include(d => d.Produto)
            .OrderByDescending(d => d.Data)
            .ToListAsync();

    public async Task<Despesa?> GetByIdAsync(int id) =>
        await db.Despesas
            .Include(d => d.Produto)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Despesa> CreateAsync(Despesa despesa)
    {
        db.Despesas.Add(despesa);
        await db.SaveChangesAsync();
        return despesa;
    }

    public async Task<Despesa> UpdateAsync(Despesa despesa)
    {
        db.Despesas.Update(despesa);
        await db.SaveChangesAsync();
        return despesa;
    }

    public async Task DeleteAsync(int id)
    {
        var despesa = await db.Despesas.FindAsync(id)
            ?? throw new KeyNotFoundException($"Despesa {id} não encontrada.");
        db.Despesas.Remove(despesa);
        await db.SaveChangesAsync();
    }

    public async Task<List<Despesa>> GetByPeriodoAsync(int mes, int ano) =>
        await db.Despesas
            .Include(d => d.Produto)
            .Where(d => d.Data.Month == mes && d.Data.Year == ano)
            .OrderByDescending(d => d.Data)
            .ToListAsync();

    public async Task MarcarPagaAsync(int id, bool pago)
    {
        var despesa = await db.Despesas.FindAsync(id)
            ?? throw new KeyNotFoundException($"Despesa {id} não encontrada.");
        despesa.Pago = pago;
        despesa.DataPagamento = pago ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
    }
}

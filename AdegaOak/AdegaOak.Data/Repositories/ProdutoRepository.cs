using AdegaOak.Models.Models;
using AdegaOak.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface IProdutoRepository
{
    Task<List<Produto>> GetAllAsync();
    Task<Produto?> GetByIdAsync(int id);
    Task<Produto> CreateAsync(Produto produto);
    Task<Produto> UpdateAsync(Produto produto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetQuantidadeAsync(int produtoId);
    Task<List<(Produto Produto, int Quantidade)>> GetEstoqueComQuantidadeAsync();
}

public class ProdutoRepository(AdegaOakDbContext db) : IProdutoRepository
{
    public async Task<List<Produto>> GetAllAsync() =>
        await db.Produtos
            .AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Bebida)
            .ToListAsync();

    public async Task<Produto?> GetByIdAsync(int id) =>
        await db.Produtos.FindAsync(id);

    public async Task<Produto> CreateAsync(Produto produto)
    {
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();
        return produto;
    }

    public async Task<Produto> UpdateAsync(Produto produto)
    {
        try
        {
            // Detach any existing tracked entity with the same ID
            var existingEntity = db.ChangeTracker.Entries<Produto>()
                .FirstOrDefault(e => e.Entity.Id == produto.Id);
            
            if (existingEntity != null)
            {
                Console.WriteLine($"[REPOSITORY] Detaching existing entity for produto {produto.Id}");
                db.Entry(existingEntity.Entity).State = EntityState.Detached;
            }
            
            // Fix DateTime Kind for PostgreSQL (must be UTC)
            produto.CriadoEm = produto.CriadoEm.ToUtc();
            
            Console.WriteLine($"[REPOSITORY] Updating produto {produto.Id}");
            db.Produtos.Update(produto);
            
            Console.WriteLine($"[REPOSITORY] Saving changes...");
            await db.SaveChangesAsync();
            
            Console.WriteLine($"[REPOSITORY] Produto {produto.Id} updated successfully");
            return produto;
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"[REPOSITORY] DbUpdateException for produto {produto.Id}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[REPOSITORY] Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REPOSITORY] Unexpected exception for produto {produto.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var produto = await db.Produtos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");
        produto.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) =>
        await db.Produtos.AnyAsync(p => p.Id == id && p.Ativo);

    public async Task<int> GetQuantidadeAsync(int produtoId)
    {
        var resultado = await db.Movimentacoes
            .Where(m => m.ProdutoId == produtoId)
            .GroupBy(m => m.Tipo)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(m => m.Quantidade) })
            .ToListAsync();

        var entradas = resultado.FirstOrDefault(r => r.Tipo == "Entrada")?.Total ?? 0;
        var saidas = resultado.FirstOrDefault(r => r.Tipo == "Saída")?.Total ?? 0;

        return entradas - saidas;
    }

    public async Task<List<(Produto Produto, int Quantidade)>> GetEstoqueComQuantidadeAsync()
    {
        var produtos = await db.Produtos
            .AsNoTracking()
            .Where(p => p.Ativo)
            .ToListAsync();

        var movimentacoes = await db.Movimentacoes
            .AsNoTracking()
            .GroupBy(m => new { m.ProdutoId, m.Tipo })
            .Select(g => new { g.Key.ProdutoId, g.Key.Tipo, Total = g.Sum(m => m.Quantidade) })
            .ToListAsync();

        return produtos.Select(p =>
        {
            var entradas = movimentacoes.FirstOrDefault(m => m.ProdutoId == p.Id && m.Tipo == "Entrada")?.Total ?? 0;
            var saidas = movimentacoes.FirstOrDefault(m => m.ProdutoId == p.Id && m.Tipo == "Saída")?.Total ?? 0;
            return (p, entradas - saidas);
        }).ToList();
    }
}

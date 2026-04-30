using AdegaOak.Models.Models;
using AdegaOak.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(Usuario usuario);
    Task DeleteAsync(int id);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
}

public class UsuarioRepository(AdegaOakDbContext db) : IUsuarioRepository
{
    public async Task<List<Usuario>> GetAllAsync() =>
        await db.Usuarios.Where(u => u.Ativo).OrderBy(u => u.Nome).ToListAsync();

    public async Task<Usuario?> GetByIdAsync(int id) =>
        await db.Usuarios.FindAsync(id);

    public async Task<Usuario?> GetByUsernameAsync(string username) =>
        await db.Usuarios.FirstOrDefaultAsync(u => u.Username == username && u.Ativo);

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        // Fix DateTime Kind for PostgreSQL (must be UTC)
        usuario.CriadoEm = usuario.CriadoEm.ToUtc();
        
        db.Usuarios.Update(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task DeleteAsync(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");
        usuario.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
    {
        var query = db.Usuarios.Where(u => u.Username == username && u.Ativo);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}

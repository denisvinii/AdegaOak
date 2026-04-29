using AdegaOak.Models.Models;
using Microsoft.EntityFrameworkCore;
using AdegaOak.Data.Data;

namespace AdegaOak.Data.Repositories;

public interface ISaldoRepository
{
    Task<SaldoConfig> GetConfigAsync();
    Task<SaldoConfig> UpdateCapitalAdminAsync(decimal valor);
}

public class SaldoRepository(AdegaOakDbContext db) : ISaldoRepository
{
    public async Task<SaldoConfig> GetConfigAsync()
    {
        var config = await db.SaldoConfigs.FindAsync(1);
        if (config == null)
        {
            config = new SaldoConfig { Id = 1, CapitalAdmin = 0 };
            db.SaldoConfigs.Add(config);
            await db.SaveChangesAsync();
        }
        return config;
    }

    public async Task<SaldoConfig> UpdateCapitalAdminAsync(decimal valor)
    {
        var config = await GetConfigAsync();
        config.CapitalAdmin = valor;
        config.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return config;
    }
}

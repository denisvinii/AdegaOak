using Adega_Oak.Services;

namespace Adega_Oak.Repositories
{
    public class SaldoRepository
    {
        private readonly DatabaseService _dbService;

        public SaldoRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public MainRepository.SaldoGeral ObterSaldoGeral()
        {
            using var conn = _dbService.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        WITH controledesaldo AS (
            SELECT
                SUM(CASE WHEN tipo = 'Saída' THEN valor_unitario * quantidade ELSE 0 END) AS total_saidas,
                SUM(CASE WHEN tipo = 'Entrada' THEN valor_unitario * quantidade ELSE 0 END) AS total_entradas
            FROM movimentacoes
            WHERE responsavel <> 'admin'
        )
        SELECT
            COALESCE(SUM(CASE WHEN m.responsavel = 'admin' THEN m.valor_unitario * m.quantidade ELSE 0 END),0) + COALESCE((c.total_saidas - c.total_entradas),0) AS capitalempresa,
            COALESCE(SUM(CASE WHEN m.responsavel = 'admin' THEN m.valor_unitario * m.quantidade ELSE 0 END),0) AS investidoporfora,
            COALESCE((c.total_saidas - c.total_entradas),0) AS saldo
        FROM movimentacoes m
        CROSS JOIN controledesaldo c
        WHERE m.responsavel = 'admin'
        GROUP BY c.total_saidas, c.total_entradas; 
    ";
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new MainRepository.SaldoGeral
                {
                    CapitalEmpresa = reader.GetDecimal(0),
                    InvestimentoPorFora = reader.GetDecimal(1),
                    Saldo = reader.GetDecimal(2)
                };
            }
            return new MainRepository.SaldoGeral();
        }
    }
}




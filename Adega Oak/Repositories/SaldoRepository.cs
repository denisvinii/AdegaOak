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
),
capital_admin AS (
    SELECT COALESCE(SUM(valor_unitario * quantidade), 0) AS valor_admin
    FROM movimentacoes
    WHERE responsavel = 'admin'
    AND tipo = 'Entrada'
),
despesas_pagas AS (
    SELECT COALESCE(SUM(valor), 0) AS total_despesas_pagas
    FROM despesas
    WHERE pago = TRUE
),
combo_pagos AS (
    SELECT COALESCE(SUM(preco_total),0) AS total_combo_vendas
    FROM combo_vendas
)
SELECT
    (total_saidas - total_entradas) + (SELECT valor_admin FROM capital_admin) - COALESCE((SELECT total_despesas_pagas FROM despesas_pagas), 0) + (SELECT total_combo_vendas FROM combo_pagos) AS capitalempresa,
    (SELECT valor_admin FROM capital_admin) AS investidoporfora,
    (total_saidas - total_entradas) - COALESCE((SELECT total_despesas_pagas FROM despesas_pagas), 0) + (SELECT total_combo_vendas FROM combo_pagos) AS saldo
FROM controledesaldo
";
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Lê como decimal para manter precisão
                decimal capitalEmpresa = reader.IsDBNull(0) ? 0m : Convert.ToDecimal(reader.GetValue(0));
                decimal investimentoPorFora = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                decimal saldo = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2));
                
                return new MainRepository.SaldoGeral
                {
                    CapitalEmpresa = capitalEmpresa,
                    InvestimentoPorFora = investimentoPorFora,
                    Saldo = saldo
                };
            }
            return new MainRepository.SaldoGeral();
        }
    }
}






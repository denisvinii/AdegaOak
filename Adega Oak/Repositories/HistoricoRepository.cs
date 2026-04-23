using Adega_Oak.Services;

namespace Adega_Oak.Repositories;

public class HistoricoRepository(DatabaseService dbService)
{
    private readonly DatabaseService _dbService = dbService;

    public List<MainRepository.Movimentacao> CarregarHistorico()
    {
        var historico = new List<MainRepository.Movimentacao>();
        bool wasOnline = _dbService.IsOnline();

        try
        {
            using var conn = _dbService.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.data, m.tipo, m.productid, m.produto, m.quantidade, m.responsavel, m.saida,
                       COALESCE(valor_unitario, valor) as valor
                FROM movimentacoes m
                LEFT JOIN estoque e ON e.productid = m.productid

                ORDER BY m.data DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var data = conn is Microsoft.Data.Sqlite.SqliteConnection ?
                    DateTime.Parse(reader.GetString(0)) :
                    reader.GetDateTime(0);

                var valor = reader.GetDecimal(7);
                var quantidade = reader.GetInt32(4);
                var valorTotal = valor * quantidade;

                historico.Add(new MainRepository.Movimentacao
                {
                    Data = data,
                    Tipo = reader.GetString(1),
                    ProductId = reader.GetInt32(2),
                    ProdutoDescricao = reader.GetString(3),
                    Quantidade = quantidade,
                    Responsavel = reader.GetString(5),
                    TipoSaida = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ValorUnitario = valor,
                    ValorTotal = valorTotal
                });
            }
        }
        catch
        {
            // Se falhou tentando online, tenta local
            if (wasOnline)
            {
                using var localConn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_dbService.SqliteDbPath}");
                localConn.Open();
                using var cmd = localConn.CreateCommand();
                cmd.CommandText = @"
                    SELECT m.data, m.tipo, m.productid, m.produto, m.quantidade, m.responsavel, m.saida,
                           COALESCE(e.valor_venda, 0) as valor
                    FROM movimentacoes m
                    LEFT JOIN estoque e ON e.productid = m.productid

                    ORDER BY m.data DESC";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var valor = reader.GetDecimal(7);
                    var quantidade = reader.GetInt32(4);
                    var valorTotal = valor * quantidade;

                    historico.Add(new MainRepository.Movimentacao
                    {
                        Data = DateTime.Parse(reader.GetString(0)),
                        Tipo = reader.GetString(1),
                        ProductId = reader.GetInt32(2),
                        ProdutoDescricao = reader.GetString(3),
                        Quantidade = quantidade,
                        Responsavel = reader.GetString(5),
                        TipoSaida = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ValorUnitario = valor,
                        ValorTotal = valorTotal
                    });
                }
            }
            else throw;
        }

        return historico;
    }

    public void ExcluirMovimentacao(int productId, DateTime data)
    {
        if (_dbService.IsOnline())
        {
            // Exclui na nuvem (PostgreSQL)
            using var conn = _dbService.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM movimentacoes WHERE productid = @productId AND data = @data";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@productId";
            paramId.Value = productId;
            cmd.Parameters.Add(paramId);

            var paramData = cmd.CreateParameter();
            paramData.ParameterName = "@data";
            paramData.Value = data; // Passa o DateTime diretamente
            if (paramData is Npgsql.NpgsqlParameter npgsqlParam)
            {
                npgsqlParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp; // Define explicitamente como timestamp
            }
            cmd.Parameters.Add(paramData);

            cmd.ExecuteNonQuery();
        }
        else
        {
            // Exclui localmente (SQLite)
            using var conn = _dbService.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM movimentacoes WHERE productid = @productId AND data = @data";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@productId";
            paramId.Value = productId;
            cmd.Parameters.Add(paramId);

            var paramData = cmd.CreateParameter();
            paramData.ParameterName = "@data";
            paramData.Value = data.ToString("yyyy-MM-dd HH:mm:ss"); // Converte para string no formato correto
            cmd.Parameters.Add(paramData);

            cmd.ExecuteNonQuery();
        }
    }

    public void RestaurarMovimentacao(MainRepository.Movimentacao mov)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO movimentacoes (data, tipo, productid, produto, quantidade, responsavel, saida) " +
                          "VALUES (@data, @tipo, @productid, @produto, @quantidade, @responsavel, @saida)";

        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@data";
        p1.Value = mov.Data;
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@tipo";
        p2.Value = mov.Tipo;
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.ParameterName = "@productid";
        p3.Value = mov.ProductId;
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.ParameterName = "@produto";
        p4.Value = mov.ProdutoDescricao;
        cmd.Parameters.Add(p4);

        var p5 = cmd.CreateParameter();
        p5.ParameterName = "@quantidade";
        p5.Value = mov.Quantidade;
        cmd.Parameters.Add(p5);

        var p6 = cmd.CreateParameter();
        p6.ParameterName = "@responsavel";
        p6.Value = mov.Responsavel;
        cmd.Parameters.Add(p6);

        var p7 = cmd.CreateParameter();
        p7.ParameterName = "@saida";
        p7.Value = mov.TipoSaida != null ? mov.TipoSaida : DBNull.Value;
        cmd.Parameters.Add(p7);

        cmd.ExecuteNonQuery();
    }

    public class RelatorioMovimentacaoDiaria
    {
        public DateTime Data { get; set; }
        public List<MainRepository.Movimentacao> Movimentacoes { get; set; } = new();
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
    }

    public RelatorioMovimentacaoDiaria GerarRelatorioDiario(DateTime data)
    {
        var relatorio = new RelatorioMovimentacaoDiaria { Data = data };
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT m.data, m.tipo, m.productid, m.produto, m.quantidade, m.responsavel, m.saida,
                   COALESCE(e.valor_venda, 0) as valor, COALESCE(m.tipo_venda, 'Varejo') as tipo_venda
            FROM movimentacoes m
            LEFT JOIN estoque e ON e.productid = m.productid
            WHERE date(m.data) = date(@data)
            AND m.tipo = 'Saída'
            AND m.valor_unitario != 0
            ORDER BY m.data";

        if (conn is Microsoft.Data.Sqlite.SqliteConnection)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = "@data";
            param.Value = data.ToString("yyyy-MM-dd");
            cmd.Parameters.Add(param);
        }
        else
        {
            var param = cmd.CreateParameter();
            param.ParameterName = "@data";
            param.Value = data.Date;
            cmd.Parameters.Add(param);
        }

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var valor = reader.GetDecimal(7);
                var quantidade = reader.GetInt32(4);
                var valorTotal = valor * quantidade;
                var tipoVenda = reader.GetString(8);

                var mov = new MainRepository.Movimentacao
                {
                    Data = conn is Microsoft.Data.Sqlite.SqliteConnection ?
                        DateTime.Parse(reader.GetString(0)) : reader.GetDateTime(0),
                    Tipo = reader.GetString(1),
                    ProductId = reader.GetInt32(2),
                    ProdutoDescricao = reader.GetString(3),
                    Quantidade = quantidade,
                    Responsavel = reader.GetString(5),
                    TipoSaida = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ValorUnitario = valor,
                    ValorTotal = valorTotal,
                    TipoVenda = tipoVenda
                };

                relatorio.Movimentacoes.Add(mov);
                relatorio.TotalSaidas += valorTotal; // Atualizamos apenas TotalSaidas já que filtramos só saídas
            }
        }

        return relatorio;
    }
}
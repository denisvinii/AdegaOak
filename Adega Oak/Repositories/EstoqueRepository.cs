using Adega_Oak.Services;

namespace Adega_Oak.Repositories;

public class EstoqueRepository
{
    private readonly DatabaseService _dbService;

    public EstoqueRepository(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public class EstoqueView
    {
        public int ProductId { get; set; }
        public string Bebida { get; set; } = "";
        public string Tamanho { get; set; } = "";
        public string Material { get; set; } = "";
        public int EstoqueMinimo { get; set; }
        public decimal Valor { get; set; }
        public decimal ValorVenda { get; set; }
        public int QuantidadeCaixa { get; set; } = 1;
        public decimal ValorCaixa { get; set; }
        public decimal ValorAtacadoCaixa { get; set; }
        public int QuantidadeMinimaAtacado { get; set; } = 20;
        public decimal ValorTotal { get; set; }
        public int Quantidade { get; set; }
    }

    // Método para obter quantidade mínima de atacado por produto
    // REMOVIDO: Este método é redundante pois CarregarEstoqueCompleto() já retorna essa informação
    // Se precisar dessa informação em outro contexto, use a query abaixo
    [Obsolete("Use CarregarEstoqueCompleto() que já inclui quantidade_minima_atacado")]
    public int ObterQuantidadeMinimaAtacado(int productId)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();

        if (conn is Npgsql.NpgsqlConnection)
        {
            cmd.CommandText = @"
                SELECT COALESCE(quantidade_minima_atacado, 20)
                FROM estoque
                WHERE productid = @productId";
        }
        else
        {
            cmd.CommandText = @"
                SELECT COALESCE(quantidade_minima_atacado, 20)
                FROM estoque
                WHERE productid = @productId";
        }

        var param = cmd.CreateParameter();
        param.ParameterName = "@productId";
        param.Value = productId;
        cmd.Parameters.Add(param);

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 20;
    }

    // Método para atualizar quantidade mínima de atacado
    public void AtualizarQuantidadeMinimaAtacado(int productId, int quantidade)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE estoque 
            SET quantidade_minima_atacado = @quantidade
            WHERE productid = @productId";
        
        var paramId = cmd.CreateParameter();
        paramId.ParameterName = "@productId";
        paramId.Value = productId;
        cmd.Parameters.Add(paramId);

        var paramQtd = cmd.CreateParameter();
        paramQtd.ParameterName = "@quantidade";
        paramQtd.Value = quantidade > 0 ? quantidade : 1;
        cmd.Parameters.Add(paramQtd);

        cmd.ExecuteNonQuery();
    }

    public List<EstoqueView> CarregarEstoqueCompleto()
    {
        var lista = new List<EstoqueView>();
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT e.productid, e.bebida, e.tamanho, e.material, 
                   e.estoque_minimo, e.valor, COALESCE(e.valor_venda, e.valor) as valor_venda,
                   COALESCE(e.quantidade_caixa, 1) as quantidade_caixa,
                   COALESCE(e.valor_caixa, e.valor_venda * e.quantidade_caixa) as valor_caixa,
                   COALESCE(e.valor_atacado_caixa, 0) as valor_atacado_caixa,
                   COALESCE(e.quantidade_minima_atacado, 1) as quantidade_minima_atacado,
                   COALESCE(SUM(CASE WHEN m.tipo = 'Entrada' THEN m.quantidade WHEN m.tipo = 'Saída' THEN -m.quantidade ELSE 0 END), 0) as quantidade
            FROM estoque e
            LEFT JOIN movimentacoes m ON e.productid = m.productid
            GROUP BY e.productid, e.bebida, e.tamanho, e.material, e.estoque_minimo, e.valor, e.valor_venda, e.quantidade_caixa, e.valor_caixa, e.valor_atacado_caixa, e.quantidade_minima_atacado";

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new EstoqueView
                {
                    ProductId = reader.GetInt32(0),
                    Bebida = reader.GetString(1),
                    Tamanho = reader.GetString(2),
                    Material = reader.GetString(3),
                    EstoqueMinimo = reader.GetInt32(4),
                    Valor = reader.GetDecimal(5),
                    ValorVenda = reader.GetDecimal(6),
                    QuantidadeCaixa = reader.GetInt32(7),
                    ValorCaixa = reader.GetDecimal(8),
                    ValorAtacadoCaixa = reader.GetDecimal(9),
                    QuantidadeMinimaAtacado = reader.IsDBNull(10) ? 20 : reader.GetInt32(10),
                    Quantidade = reader.GetInt32(11),
                    ValorTotal = reader.GetDecimal(6) * reader.GetInt32(11)
                });
            }
        }
        return lista;
    }

    public List<MainRepository.Produto> CarregarEstoque()
    {
        var produtos = new List<MainRepository.Produto>();
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT productid, bebida, tamanho, material FROM estoque";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            produtos.Add(new MainRepository.Produto
            {
                ProductId = reader.GetInt32(0),
                Bebida = reader.GetString(1),
                Tamanho = reader.GetString(2),
                Material = reader.GetString(3)
            });
        }
        return produtos;
    }

    public void AtualizarPrecoVenda(string descricao, decimal novoPrecoVenda)
    {
        var partes = descricao.Split(" - ");
        if (partes.Length != 3)
            return;

        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE estoque 
            SET valor_venda = @novoPrecoVenda 
            WHERE bebida = @bebida 
            AND tamanho = @tamanho 
            AND material = @material";

        var paramPreco = cmd.CreateParameter();
        paramPreco.ParameterName = "@novoPrecoVenda";
        paramPreco.Value = novoPrecoVenda;
        cmd.Parameters.Add(paramPreco);

        var paramBebida = cmd.CreateParameter();
        paramBebida.ParameterName = "@bebida";
        paramBebida.Value = partes[0];
        cmd.Parameters.Add(paramBebida);

        var paramTamanho = cmd.CreateParameter();
        paramTamanho.ParameterName = "@tamanho";
        paramTamanho.Value = partes[1];
        cmd.Parameters.Add(paramTamanho);

        var paramMaterial = cmd.CreateParameter();
        paramMaterial.ParameterName = "@material";
        paramMaterial.Value = partes[2];
        cmd.Parameters.Add(paramMaterial);

        cmd.ExecuteNonQuery();
    }

    public void AtualizarCaixaPreco(string descricao, int quantidadeCaixa, decimal valorCaixa)
    {
        var partes = descricao.Split(" - ");
        if (partes.Length != 3)
            return;

        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE estoque 
            SET quantidade_caixa = @quantidadeCaixa,
                valor_caixa = @valorCaixa
            WHERE bebida = @bebida 
            AND tamanho = @tamanho 
            AND material = @material";

        var paramQtd = cmd.CreateParameter();
        paramQtd.ParameterName = "@quantidadeCaixa";
        paramQtd.Value = quantidadeCaixa > 0 ? quantidadeCaixa : 1;
        cmd.Parameters.Add(paramQtd);

        var paramValor = cmd.CreateParameter();
        paramValor.ParameterName = "@valorCaixa";
        paramValor.Value = valorCaixa;
        cmd.Parameters.Add(paramValor);

        var paramBebida = cmd.CreateParameter();
        paramBebida.ParameterName = "@bebida";
        paramBebida.Value = partes[0];
        cmd.Parameters.Add(paramBebida);

        var paramTamanho = cmd.CreateParameter();
        paramTamanho.ParameterName = "@tamanho";
        paramTamanho.Value = partes[1];
        cmd.Parameters.Add(paramTamanho);

        var paramMaterial = cmd.CreateParameter();
        paramMaterial.ParameterName = "@material";
        paramMaterial.Value = partes[2];
        cmd.Parameters.Add(paramMaterial);

        cmd.ExecuteNonQuery();
    }

    public void AtualizarPrecoAtacado(string descricao, decimal valorAtacadoCaixa)
    {
        var partes = descricao.Split(" - ");
        if (partes.Length != 3)
            return;

        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE estoque 
            SET valor_atacado_caixa = @valorAtacadoCaixa
            WHERE bebida = @bebida 
            AND tamanho = @tamanho 
            AND material = @material";

        var paramValor = cmd.CreateParameter();
        paramValor.ParameterName = "@valorAtacadoCaixa";
        paramValor.Value = valorAtacadoCaixa;
        cmd.Parameters.Add(paramValor);

        var paramBebida = cmd.CreateParameter();
        paramBebida.ParameterName = "@bebida";
        paramBebida.Value = partes[0];
        cmd.Parameters.Add(paramBebida);

        var paramTamanho = cmd.CreateParameter();
        paramTamanho.ParameterName = "@tamanho";
        paramTamanho.Value = partes[1];
        cmd.Parameters.Add(paramTamanho);

        var paramMaterial = cmd.CreateParameter();
        paramMaterial.ParameterName = "@material";
        paramMaterial.Value = partes[2];
        cmd.Parameters.Add(paramMaterial);

        cmd.ExecuteNonQuery();
    }
}
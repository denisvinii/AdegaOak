using Adega_Oak.Services;
using System;
using System.Data;

namespace Adega_Oak.Repositories;


public class MainRepository(DatabaseService dbService)
{
    private readonly DatabaseService _dbService = dbService;

    public class Produto
    {
        public int ProductId { get; set; }
        public string Bebida { get; set; } = "";
        public string Tamanho { get; set; } = "";
        public string Material { get; set; } = "";
        public string Descricao => $"{Bebida} - {Tamanho} - {Material}";
    }

    public class Movimentacao
    {
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = "";
        public int ProductId { get; set; }
        public string ProdutoDescricao { get; set; } = "";
        public int Quantidade { get; set; }
        public string Responsavel { get; set; } = "";
        public string? TipoSaida { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }

    private void RegistrarHistoricoValor(IDbConnection conn, int productId, decimal valor, DateTime data)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO historico_valores (data, productid, valor)
            VALUES (@data, @productid, @valor)";

        var paramData = cmd.CreateParameter();
        paramData.ParameterName = "@data";
        if (conn is Microsoft.Data.Sqlite.SqliteConnection)
            paramData.Value = data.ToString("yyyy-MM-dd HH:mm:ss");
        else
            paramData.Value = data;
        cmd.Parameters.Add(paramData);

        var paramProductId = cmd.CreateParameter();
        paramProductId.ParameterName = "@productid";
        paramProductId.Value = productId;
        cmd.Parameters.Add(paramProductId);

        var paramValor = cmd.CreateParameter();
        paramValor.ParameterName = "@valor";
        paramValor.Value = valor;
        cmd.Parameters.Add(paramValor);

        cmd.ExecuteNonQuery();
    }

    public List<Produto> CarregarProdutos()
    {
        var produtos = new List<Produto>();
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT productid, bebida, tamanho, material FROM estoque";
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    ProductId = reader.GetInt32(0),
                    Bebida = reader.GetString(1),
                    Tamanho = reader.GetString(2),
                    Material = reader.GetString(3)
                });
            }
        }
        return produtos;
    }

    public List<string> CarregarResponsaveis()
    {
        var responsaveis = new List<string>();
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT username FROM funcionarios";
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                responsaveis.Add(reader.GetString(0));
            }
        }
        return responsaveis;
    }

    public Produto AdicionarProduto(Produto produto)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();

        // Verifica se já existe produto igual
        cmd.CommandText = @"SELECT productid, bebida, tamanho, material FROM estoque WHERE bebida = @bebida AND tamanho = @tamanho AND material = @material";
        var pBebida = cmd.CreateParameter();
        pBebida.ParameterName = "@bebida";
        pBebida.Value = produto.Bebida;
        cmd.Parameters.Add(pBebida);
        var pTamanho = cmd.CreateParameter();
        pTamanho.ParameterName = "@tamanho";
        pTamanho.Value = produto.Tamanho;
        cmd.Parameters.Add(pTamanho);
        var pMaterial = cmd.CreateParameter();
        pMaterial.ParameterName = "@material";
        pMaterial.Value = produto.Material;
        cmd.Parameters.Add(pMaterial);

        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                // Produto já existe, retorna o existente
                return new Produto
                {
                    ProductId = reader.GetInt32(0),
                    Bebida = reader.GetString(1),
                    Tamanho = reader.GetString(2),
                    Material = reader.GetString(3)
                };
            }
        }
        cmd.Parameters.Clear();

        // Se não existe, insere novo produto
        cmd.CommandText = "SELECT MAX(productid) + 1 FROM estoque";
        var nextId = Convert.ToInt32(cmd.ExecuteScalar());

        cmd.CommandText = @"
            INSERT INTO estoque (productid, bebida, tamanho, material, valor)
            VALUES (@productid, @bebida, @tamanho, @material, 0)
            RETURNING productid";

        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@productid";
        p1.Value = nextId;
        cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@bebida";
        p2.Value = produto.Bebida;
        cmd.Parameters.Add(p2);
        var p3 = cmd.CreateParameter();
        p3.ParameterName = "@tamanho";
        p3.Value = produto.Tamanho;
        cmd.Parameters.Add(p3);
        var p4 = cmd.CreateParameter();
        p4.ParameterName = "@material";
        p4.Value = produto.Material;
        cmd.Parameters.Add(p4);

        cmd.ExecuteNonQuery();

        produto.ProductId = nextId;
        return produto;
    }

    public void RegistrarMovimentacao(Movimentacao mov)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();

        // Verifica se o produto já existe no estoque
        cmd.CommandText = "SELECT COUNT(*) FROM estoque WHERE productid = @productid";
        var paramProductIdCheck = cmd.CreateParameter();
        paramProductIdCheck.ParameterName = "@productid";
        paramProductIdCheck.Value = mov.ProductId;
        cmd.Parameters.Add(paramProductIdCheck);

        var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        cmd.Parameters.Clear();

        if (!exists)
        {
            // Insere o novo produto no estoque
            cmd.CommandText = @"
                INSERT INTO estoque (productid, bebida, tamanho, material, valor) 
                VALUES (@productid, @bebida, @tamanho, @material, @valor)";

            var paramBebida = cmd.CreateParameter();
            paramBebida.ParameterName = "@bebida";
            paramBebida.Value = mov.ProdutoDescricao.Split(' ')[0]; // Exemplo simplificado
            cmd.Parameters.Add(paramBebida);

            var paramTamanho = cmd.CreateParameter();
            paramTamanho.ParameterName = "@tamanho";
            paramTamanho.Value = mov.ProdutoDescricao.Split(' ')[1]; // Exemplo simplificado
            cmd.Parameters.Add(paramTamanho);

            var paramMaterial = cmd.CreateParameter();
            paramMaterial.ParameterName = "@material";
            paramMaterial.Value = mov.ProdutoDescricao.Split(' ')[2]; // Exemplo simplificado
            cmd.Parameters.Add(paramMaterial);

            var paramValor = cmd.CreateParameter();
            paramValor.ParameterName = "@valor";
            paramValor.Value = mov.ValorUnitario;
            cmd.Parameters.Add(paramValor);

            var paramProductId = cmd.CreateParameter();
            paramProductId.ParameterName = "@productid";
            paramProductId.Value = mov.ProductId;
            cmd.Parameters.Add(paramProductId);

            cmd.ExecuteNonQuery();
        }
        else if (mov.Tipo == "Entrada")
        {
            // Atualiza o valor do produto no estoque para o valor mais recente
            cmd.CommandText = "UPDATE estoque SET valor = @valor WHERE productid = @productid";
            var paramValor = cmd.CreateParameter();
            paramValor.ParameterName = "@valor";
            paramValor.Value = mov.ValorUnitario;
            cmd.Parameters.Add(paramValor);
            var paramProductId = cmd.CreateParameter();
            paramProductId.ParameterName = "@productid";
            paramProductId.Value = mov.ProductId;
            cmd.Parameters.Add(paramProductId);
            cmd.ExecuteNonQuery();
        }

        // Registra o valor no histórico de valores (sempre que for entrada)
        if (mov.Tipo == "Entrada")
        {
            RegistrarHistoricoValor(conn, mov.ProductId, mov.ValorUnitario, mov.Data);
        }

        cmd.Parameters.Clear();
        // Registra a movimentação
        cmd.CommandText = @"
            INSERT INTO movimentacoes (data, tipo, productid, produto, quantidade, responsavel, saida, valor_unitario) 
            VALUES (@data, @tipo, @productid, @produto, @quantidade, @responsavel, @saida, @valor_unitario)";

        var paramData = cmd.CreateParameter();
        paramData.ParameterName = "@data";
        paramData.Value = mov.Data;
        cmd.Parameters.Add(paramData);

        var paramTipo = cmd.CreateParameter();
        paramTipo.ParameterName = "@tipo";
        paramTipo.Value = mov.Tipo;
        cmd.Parameters.Add(paramTipo);

        var paramProdutoID = cmd.CreateParameter();
        paramProdutoID.ParameterName = "@productid";
        paramProdutoID.Value = mov.ProductId;
        cmd.Parameters.Add(paramProdutoID);

        var paramProduto = cmd.CreateParameter();
        paramProduto.ParameterName = "@produto";
        paramProduto.Value = mov.ProdutoDescricao;
        cmd.Parameters.Add(paramProduto);

        var paramQuantidadeMov = cmd.CreateParameter();
        paramQuantidadeMov.ParameterName = "@quantidade";
        paramQuantidadeMov.Value = mov.Quantidade;
        cmd.Parameters.Add(paramQuantidadeMov);

        var paramResponsavel = cmd.CreateParameter();
        paramResponsavel.ParameterName = "@responsavel";
        paramResponsavel.Value = mov.Responsavel;
        cmd.Parameters.Add(paramResponsavel);

        var paramSaida = cmd.CreateParameter();
        paramSaida.ParameterName = "@saida";
        paramSaida.Value = mov.TipoSaida != null ? mov.TipoSaida : (object)DBNull.Value;
        cmd.Parameters.Add(paramSaida);

        var paramValorUnitario = cmd.CreateParameter();
        paramValorUnitario.ParameterName = "@valor_unitario";
        paramValorUnitario.Value = mov.ValorUnitario;
        cmd.Parameters.Add(paramValorUnitario);

        cmd.ExecuteNonQuery();
    }

    public void ExcluirProduto(int productId)
    {
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM estoque WHERE productid = @productid";
        var param = cmd.CreateParameter();
        param.ParameterName = "@productid";
        param.Value = productId;
        cmd.Parameters.Add(param);
        cmd.ExecuteNonQuery();
    }
}
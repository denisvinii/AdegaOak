using Adega_Oak.Services;

namespace Adega_Oak.Repositories;

public class EstoqueRepository(DatabaseService dbService)
{
    private readonly DatabaseService _dbService = dbService;

    public class EstoqueView
    {
        public int ProductId { get; set; }
        public string Bebida { get; set; } = "";
        public string Tamanho { get; set; } = "";
        public string Material { get; set; } = "";
        public int Quantidade { get; set; }
        public int EstoqueMinimo { get; set; }
        public decimal Valor { get; set; }
    }

    public List<EstoqueView> CarregarEstoqueCompleto()
    {
        var lista = new List<EstoqueView>();
        using var conn = _dbService.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                e.productid, 
                e.bebida, 
                e.tamanho, 
                e.material, 
                COALESCE(e.estoque_minimo, 0) as estoque_minimo,
                COALESCE(e.valor, 0) as valor,
                (
                    COALESCE(SUM(CASE WHEN m.tipo = 'Entrada' THEN m.quantidade ELSE 0 END), 0) -
                    COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN m.quantidade ELSE 0 END), 0)
                ) as quantidade
            FROM estoque e
            LEFT JOIN movimentacoes m ON m.productid = e.productid
            GROUP BY e.productid, e.bebida, e.tamanho, e.material, e.estoque_minimo, e.valor
            ORDER BY e.bebida, e.tamanho, e.material;";

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
                    Quantidade = reader.GetInt32(6)
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
}
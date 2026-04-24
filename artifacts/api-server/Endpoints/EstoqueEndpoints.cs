using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class EstoqueEndpoints
{
    private const string SelectBase = @"
SELECT
    e.productid, e.bebida, e.tamanho, e.material,
    e.valor::float8 AS valor,
    e.valor_venda::float8 AS valor_venda,
    e.quantidade_caixa,
    e.valor_caixa::float8 AS valor_caixa,
    e.valor_atacado_caixa::float8 AS valor_atacado_caixa,
    e.estoque_minimo,
    e.quantidade_minima_atacado,
    e.ativo,
    COALESCE((
        SELECT SUM(CASE WHEN m.tipo = 'Entrada' THEN m.quantidade
                        WHEN m.tipo = 'Saída'   THEN -m.quantidade
                        ELSE 0 END)
        FROM movimentacoes m WHERE m.productid = e.productid
    ), 0)::int AS quantidade
FROM estoque e
";

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/estoque", async (string? search, string? low) =>
        {
            var args = new List<object?>();
            var where = new List<string> { "COALESCE(e.ativo, true) = true" };
            if (!string.IsNullOrWhiteSpace(search))
            {
                args.Add($"%{search.Trim()}%");
                where.Add($"(e.bebida ILIKE ${args.Count} OR e.tamanho ILIKE ${args.Count} OR e.material ILIKE ${args.Count})");
            }
            var sql = $"{SelectBase} WHERE {string.Join(" AND ", where)} ORDER BY e.bebida, e.tamanho";
            if (Coerce.AsBool(low) == true)
                sql = $"SELECT * FROM ({sql}) sub WHERE quantidade <= sub.estoque_minimo";

            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(sql, args.ToArray());
            return Results.Ok(rows);
        });

        app.MapGet("/api/estoque/{productid:int}", async (int productid) =>
        {
            await using var conn = await Db.OpenAsync();
            var row = await conn.QueryFirstOrDefaultAsync($"{SelectBase} WHERE e.productid = $1", productid);
            return row is null
                ? Results.NotFound(new { error = "Produto não encontrado" })
                : Results.Ok(row);
        });

        app.MapPost("/api/estoque", async (Dictionary<string, JsonElement>? body) =>
        {
            await using var conn = await Db.OpenAsync();
            var insert = await conn.QueryAsync(@"
INSERT INTO estoque
    (bebida, tamanho, material, valor, valor_venda, quantidade_caixa, valor_caixa,
     valor_atacado_caixa, estoque_minimo, quantidade_minima_atacado, ativo, criado_em)
VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,true, NOW())
RETURNING productid",
                Coerce.AsString(Coerce.Get(body, "bebida")),
                Coerce.AsString(Coerce.Get(body, "tamanho")),
                Coerce.AsString(Coerce.Get(body, "material")),
                Coerce.AsNum(Coerce.Get(body, "valor")),
                Coerce.AsNum(Coerce.Get(body, "valor_venda")),
                Coerce.AsInt(Coerce.Get(body, "quantidade_caixa"), 1),
                Coerce.AsNum(Coerce.Get(body, "valor_caixa"), 0),
                Coerce.AsNum(Coerce.Get(body, "valor_atacado_caixa"), 0),
                Coerce.AsInt(Coerce.Get(body, "estoque_minimo"), 0),
                Coerce.AsInt(Coerce.Get(body, "quantidade_minima_atacado"), 20));

            var newId = Convert.ToInt32(insert[0]["productid"]);
            var detail = await conn.QueryFirstOrDefaultAsync($"{SelectBase} WHERE e.productid = $1", newId);
            return Results.Created($"/api/estoque/{newId}", detail);
        });

        app.MapMethods("/api/estoque/{productid:int}", new[] { "PATCH" }, async (int productid, Dictionary<string, JsonElement>? body) =>
        {
            string[] allow =
            {
                "bebida","tamanho","material","valor","valor_venda","quantidade_caixa",
                "valor_caixa","valor_atacado_caixa","estoque_minimo","quantidade_minima_atacado"
            };
            var sets = new List<string>();
            var args = new List<object?>();
            foreach (var k in allow)
            {
                var v = Coerce.Get(body, k);
                if (v is null) continue;
                object? typed = k switch
                {
                    "bebida" or "tamanho" or "material" => Coerce.AsString(v),
                    "quantidade_caixa" or "estoque_minimo" or "quantidade_minima_atacado" => Coerce.AsInt(v),
                    _ => Coerce.AsNum(v),
                };
                args.Add(typed);
                sets.Add($"{k} = ${args.Count}");
            }
            await using var conn = await Db.OpenAsync();
            if (sets.Count > 0)
            {
                args.Add(productid);
                await conn.ExecuteAsync(
                    $"UPDATE estoque SET {string.Join(", ", sets)} WHERE productid = ${args.Count}",
                    args.ToArray());
            }
            var detail = await conn.QueryFirstOrDefaultAsync($"{SelectBase} WHERE e.productid = $1", productid);
            return detail is null
                ? Results.NotFound(new { error = "Produto não encontrado" })
                : Results.Ok(detail);
        });

        app.MapDelete("/api/estoque/{productid:int}", async (int productid) =>
        {
            await using var conn = await Db.OpenAsync();
            await conn.ExecuteAsync("UPDATE estoque SET ativo = false WHERE productid = $1", productid);
            return Results.NoContent();
        });
    }
}

using System.Text.Json;
using AdegaOak.Api.Infra;
using Npgsql;

namespace AdegaOak.Api.Endpoints;

public static class CombosEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/combos", async () =>
        {
            await using var conn = await Db.OpenAsync();
            var ids = await conn.QueryAsync("SELECT combo_id FROM combos ORDER BY nome");
            var result = new List<Dictionary<string, object?>>();
            foreach (var row in ids)
            {
                var combo = await LoadCombo(conn, Convert.ToInt32(row["combo_id"]));
                if (combo is not null) result.Add(combo);
            }
            return Results.Ok(result);
        });

        app.MapGet("/api/combos/{combo_id:int}", async (int combo_id) =>
        {
            await using var conn = await Db.OpenAsync();
            var combo = await LoadCombo(conn, combo_id);
            return combo is null
                ? Results.NotFound(new { error = "Combo não encontrado" })
                : Results.Ok(combo);
        });

        app.MapPost("/api/combos", async (Dictionary<string, JsonElement>? body) =>
        {
            await using var conn = await Db.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            try
            {
                var inserted = await tx.QueryAsync(@"
INSERT INTO combos (nome, descricao, preco_venda, ativo, data_criacao)
VALUES ($1,$2,$3,$4,NOW()) RETURNING combo_id",
                    Coerce.AsString(Coerce.Get(body, "nome")),
                    Coerce.AsString(Coerce.Get(body, "descricao")),
                    Coerce.AsNum(Coerce.Get(body, "preco_venda")),
                    Coerce.AsBool(Coerce.Get(body, "ativo")) ?? true);

                var newId = Convert.ToInt32(inserted[0]["combo_id"]);
                await InsertComposicao(tx, newId, body);
                await tx.CommitAsync();
                var combo = await LoadCombo(conn, newId);
                return Results.Created($"/api/combos/{newId}", combo);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });

        app.MapMethods("/api/combos/{combo_id:int}", new[] { "PATCH" }, async (int combo_id, Dictionary<string, JsonElement>? body) =>
        {
            await using var conn = await Db.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            try
            {
                string[] keys = { "nome", "descricao", "preco_venda", "ativo" };
                var sets = new List<string>();
                var args = new List<object?>();
                foreach (var k in keys)
                {
                    var v = Coerce.Get(body, k);
                    if (v is null) continue;
                    object? typed = k switch
                    {
                        "preco_venda" => Coerce.AsNum(v),
                        "ativo" => Coerce.AsBool(v),
                        _ => Coerce.AsString(v),
                    };
                    args.Add(typed);
                    sets.Add($"{k} = ${args.Count}");
                }
                if (sets.Count > 0)
                {
                    args.Add(combo_id);
                    await tx.ExecuteAsync(
                        $"UPDATE combos SET {string.Join(", ", sets)} WHERE combo_id = ${args.Count}",
                        args.ToArray());
                }
                if (body is not null && body.TryGetValue("composicao", out var compEl) && compEl.ValueKind == JsonValueKind.Array)
                {
                    await tx.ExecuteAsync("DELETE FROM combo_composicao WHERE combo_id = $1", combo_id);
                    await InsertComposicao(tx, combo_id, body);
                }
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            var combo = await LoadCombo(conn, combo_id);
            return Results.Ok(combo);
        });

        app.MapDelete("/api/combos/{combo_id:int}", async (int combo_id) =>
        {
            await using var conn = await Db.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM combo_composicao WHERE combo_id = $1", combo_id);
            await conn.ExecuteAsync("DELETE FROM combos WHERE combo_id = $1", combo_id);
            return Results.NoContent();
        });
    }

    private static async Task<Dictionary<string, object?>?> LoadCombo(NpgsqlConnection conn, int comboId)
    {
        var head = await conn.QueryFirstOrDefaultAsync(@"
SELECT combo_id, nome, descricao, preco_venda::float8 AS preco_venda, ativo, data_criacao
FROM combos WHERE combo_id = $1", comboId);
        if (head is null) return null;

        var comp = await conn.QueryAsync(@"
SELECT cc.composicao_id, cc.product_id,
       (e.bebida || ' ' || COALESCE(e.tamanho,'')) AS produto,
       cc.quantidade::float8 AS quantidade, cc.unidade, cc.debita_estoque,
       e.valor::float8 AS custo_unitario
FROM combo_composicao cc
LEFT JOIN estoque e ON e.productid = cc.product_id
WHERE cc.combo_id = $1", comboId);

        var composicao = comp.Select(r => new Dictionary<string, object?>
        {
            ["composicao_id"] = r["composicao_id"],
            ["product_id"] = r["product_id"],
            ["produto"] = r["produto"],
            ["quantidade"] = r["quantidade"],
            ["unidade"] = r["unidade"],
            ["debita_estoque"] = r["debita_estoque"],
        }).ToList();

        double custoTotal = 0;
        foreach (var r in comp)
        {
            var custo = Convert.ToDouble(r["custo_unitario"] ?? 0);
            var qty = Convert.ToDouble(r["quantidade"] ?? 0);
            custoTotal += custo * qty;
        }

        head["composicao"] = composicao;
        head["custo_total"] = custoTotal;
        return head;
    }

    private static async Task InsertComposicao(NpgsqlTransaction tx, int comboId, Dictionary<string, JsonElement>? body)
    {
        if (body is null || !body.TryGetValue("composicao", out var compEl) || compEl.ValueKind != JsonValueKind.Array) return;
        foreach (var item in compEl.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            object? productId = item.TryGetProperty("product_id", out var pid) ? Coerce.AsInt(pid) : null;
            object? quantidade = item.TryGetProperty("quantidade", out var q) ? Coerce.AsNum(q) : null;
            object? unidade = item.TryGetProperty("unidade", out var u) ? Coerce.AsString(u) : null;
            object? debita = item.TryGetProperty("debita_estoque", out var d) ? (Coerce.AsBool(d) ?? true) : true;
            await tx.ExecuteAsync(
                "INSERT INTO combo_composicao (combo_id, product_id, quantidade, unidade, debita_estoque) " +
                "VALUES ($1,$2,$3,$4,$5)",
                comboId, productId, quantidade, unidade, debita);
        }
    }
}

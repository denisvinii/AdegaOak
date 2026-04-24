using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class VendasEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/vendas/combo", async (Dictionary<string, JsonElement>? body) =>
        {
            var comboId = Coerce.AsInt(Coerce.Get(body, "combo_id"));
            var quantidade = Coerce.AsInt(Coerce.Get(body, "quantidade"));
            if (comboId is null || quantidade is null || quantidade <= 0)
                return Results.BadRequest(new { error = "combo_id/quantidade inválidos" });

            var responsavel = Coerce.AsString(Coerce.Get(body, "responsavel")) ?? "";
            var observacoes = Coerce.AsString(Coerce.Get(body, "observacoes"));
            var precoUnitarioOverride = Coerce.AsNum(Coerce.Get(body, "preco_unitario"));

            await using var conn = await Db.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            try
            {
                var combo = await tx.QueryFirstOrDefaultAsync(
                    "SELECT nome, preco_venda::float8 AS preco_venda FROM combos WHERE combo_id = $1",
                    comboId);
                if (combo is null)
                {
                    await tx.RollbackAsync();
                    return Results.NotFound(new { error = "Combo não encontrado" });
                }
                var precoUnitario = precoUnitarioOverride ?? Convert.ToDouble(combo["preco_venda"] ?? 0);
                var precoTotal = precoUnitario * quantidade.Value;

                var venda = await tx.QueryAsync(@"
INSERT INTO combo_vendas (combo_id, quantidade, preco_unitario, preco_total, data_venda, responsavel, observacoes, tipo_movimento)
VALUES ($1,$2,$3,$4,NOW(),$5,$6,'Saida') RETURNING venda_id, data_venda",
                    comboId, quantidade, precoUnitario, precoTotal, responsavel, observacoes);

                var comp = await tx.QueryAsync(@"
SELECT cc.product_id, cc.quantidade::float8 AS quantidade, cc.debita_estoque,
       (e.bebida || ' ' || COALESCE(e.tamanho,'')) AS produto,
       e.valor_venda::float8 AS valor_venda
FROM combo_composicao cc LEFT JOIN estoque e ON e.productid = cc.product_id
WHERE cc.combo_id = $1", comboId);

                foreach (var it in comp)
                {
                    var debita = Coerce.AsBool(it["debita_estoque"]) ?? true;
                    if (!debita) continue;
                    var perCombo = Convert.ToDouble(it["quantidade"] ?? 0);
                    var qty = (int)Math.Ceiling(perCombo * quantidade.Value);
                    if (qty <= 0) continue;
                    await tx.ExecuteAsync(@"
INSERT INTO movimentacoes
    (data, tipo, tipo_venda, productid, produto, quantidade, responsavel, saida, valor_unitario, observacoes)
VALUES (NOW(),'Saída','Varejo',$1,$2,$3,$4,$5,$6,$7)",
                        it["product_id"], it["produto"], qty, responsavel,
                        $"Combo #{comboId}",
                        it["valor_venda"] ?? 0,
                        $"Venda combo {combo["nome"]}");
                }

                await tx.CommitAsync();

                return Results.Created("/api/vendas/combo", new Dictionary<string, object?>
                {
                    ["venda_id"] = venda[0]["venda_id"],
                    ["combo_id"] = comboId,
                    ["nome"] = combo["nome"],
                    ["quantidade"] = quantidade,
                    ["preco_unitario"] = precoUnitario,
                    ["preco_total"] = precoTotal,
                    ["data_venda"] = venda[0]["data_venda"],
                    ["responsavel"] = responsavel,
                    ["observacoes"] = observacoes,
                    ["tipo_movimento"] = "Saida",
                });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });

        app.MapGet("/api/vendas/combo/list", async (string? from, string? to) =>
        {
            var args = new List<object?>();
            var where = new List<string>();
            if (!string.IsNullOrEmpty(from)) { args.Add(from); where.Add($"v.data_venda >= ${args.Count}"); }
            if (!string.IsNullOrEmpty(to)) { args.Add(to); where.Add($"v.data_venda <= ${args.Count}"); }

            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync($@"
SELECT v.venda_id, v.combo_id, c.nome, v.quantidade,
       v.preco_unitario::float8 AS preco_unitario,
       v.preco_total::float8 AS preco_total,
       v.data_venda, v.responsavel, v.observacoes, v.tipo_movimento
FROM combo_vendas v LEFT JOIN combos c ON c.combo_id = v.combo_id
{(where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")}
ORDER BY v.data_venda DESC LIMIT 500", args.ToArray());
            return Results.Ok(rows);
        });
    }
}

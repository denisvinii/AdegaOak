using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class MovimentacoesEndpoints
{
    private const string SelectBase = @"
SELECT
    m.id, m.data, m.tipo, m.tipo_venda, m.productid, m.produto, m.quantidade,
    m.responsavel, m.saida,
    m.valor_unitario::float8 AS valor_unitario,
    (m.valor_unitario * m.quantidade)::float8 AS valor_total,
    m.observacoes
FROM movimentacoes m
";

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/movimentacoes", async (string? tipo, string? from, string? to, string? productid, string? limit) =>
        {
            var args = new List<object?>();
            var where = new List<string>();
            if (!string.IsNullOrEmpty(tipo) && tipo != "All") { args.Add(tipo); where.Add($"m.tipo = ${args.Count}"); }
            if (!string.IsNullOrEmpty(from)) { args.Add(from); where.Add($"m.data >= ${args.Count}"); }
            if (!string.IsNullOrEmpty(to)) { args.Add(to); where.Add($"m.data <= ${args.Count}"); }
            var pid = Coerce.AsInt(productid);
            if (pid is not null) { args.Add(pid); where.Add($"m.productid = ${args.Count}"); }
            var lim = Coerce.AsInt(limit, 200) ?? 200;
            var sql = $"{SelectBase} {(where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")} ORDER BY m.data DESC LIMIT {lim}";

            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(sql, args.ToArray());
            return Results.Ok(rows);
        });

        app.MapPost("/api/movimentacoes", async (Dictionary<string, JsonElement>? body) =>
        {
            var tipo = Coerce.AsString(Coerce.Get(body, "tipo"));
            var tipoVenda = Coerce.AsString(Coerce.Get(body, "tipo_venda")) ?? "Varejo";
            var productid = Coerce.AsInt(Coerce.Get(body, "productid"));
            var quantidade = Coerce.AsNum(Coerce.Get(body, "quantidade"));
            var responsavel = Coerce.AsString(Coerce.Get(body, "responsavel")) ?? "";
            var valorUnitario = Coerce.AsNum(Coerce.Get(body, "valor_unitario"));
            var saida = Coerce.AsString(Coerce.Get(body, "saida"));
            var observacoes = Coerce.AsString(Coerce.Get(body, "observacoes"));

            if (tipo is not "Entrada" and not "Saída")
                return Results.BadRequest(new { error = "tipo inválido" });
            if (productid is null || quantidade is null || quantidade <= 0)
                return Results.BadRequest(new { error = "produto/quantidade inválidos" });
            if (valorUnitario is null) valorUnitario = 0;

            await using var conn = await Db.OpenAsync();
            var prod = await conn.QueryFirstOrDefaultAsync(
                "SELECT bebida, tamanho, quantidade_caixa FROM estoque WHERE productid = $1",
                productid);
            if (prod is null)
                return Results.NotFound(new { error = "Produto não encontrado" });

            var produto = $"{prod["bebida"]} {prod["tamanho"]}".Trim();
            var qcaixa = Convert.ToInt32(prod["quantidade_caixa"] ?? 1);
            if (qcaixa < 1) qcaixa = 1;

            // Atacado: o usuário informa CAIXAS e VALOR POR CAIXA. Estoque é em unidades
            // individuais, então expandimos para `quantidade × quantidade_caixa` e
            // ajustamos o valor unitário para que o total bruto permaneça igual.
            if (tipo == "Saída" && tipoVenda == "Atacado" && qcaixa > 1)
            {
                var caixas = quantidade.Value;
                var totalBruto = quantidade.Value * valorUnitario.Value;
                quantidade = caixas * qcaixa;
                valorUnitario = quantidade > 0 ? totalBruto / quantidade : 0;
                var tag = $"Atacado: {caixas} caixa(s) × {qcaixa} un.";
                observacoes = string.IsNullOrEmpty(observacoes) ? tag : $"{tag} — {observacoes}";
            }

            var insert = await conn.QueryAsync(@"
INSERT INTO movimentacoes
    (data, tipo, tipo_venda, productid, produto, quantidade, responsavel, saida, valor_unitario, observacoes)
VALUES (NOW(), $1,$2,$3,$4,$5,$6,$7,$8,$9) RETURNING id",
                tipo, tipoVenda, productid, produto, quantidade, responsavel,
                string.IsNullOrEmpty(saida) ? null : saida,
                valorUnitario,
                observacoes);

            var newId = Convert.ToInt32(insert[0]["id"]);
            var detail = await conn.QueryFirstOrDefaultAsync($"{SelectBase} WHERE m.id = $1", newId);
            return Results.Created($"/api/movimentacoes/{newId}", detail);
        });

        app.MapDelete("/api/movimentacoes/{id:int}", async (int id) =>
        {
            await using var conn = await Db.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM movimentacoes WHERE id = $1", id);
            return Results.NoContent();
        });
    }
}

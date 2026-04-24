using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class DespesasEndpoints
{
    private const string Select = @"
SELECT id, descricao, valor::float8 AS valor, data, tipo, pago, data_pagamento, notas
FROM despesas
";

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/despesas", async (string? pago, string? from, string? to) =>
        {
            var args = new List<object?>();
            var where = new List<string>();
            var pagoVal = Coerce.AsBool(pago);
            if (pagoVal is not null) { args.Add(pagoVal); where.Add($"pago = ${args.Count}"); }
            if (!string.IsNullOrEmpty(from)) { args.Add(from); where.Add($"data >= ${args.Count}"); }
            if (!string.IsNullOrEmpty(to)) { args.Add(to); where.Add($"data <= ${args.Count}"); }

            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(
                $"{Select} {(where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")} ORDER BY data DESC",
                args.ToArray());
            return Results.Ok(rows);
        });

        app.MapPost("/api/despesas", async (Dictionary<string, JsonElement>? body) =>
        {
            await using var conn = await Db.OpenAsync();
            var inserted = await conn.QueryAsync(@"
INSERT INTO despesas (descricao, valor, data, tipo, pago, data_pagamento, notas, criado_em)
VALUES ($1,$2,$3,$4,$5,$6,$7,NOW()) RETURNING id",
                Coerce.AsString(Coerce.Get(body, "descricao")),
                Coerce.AsNum(Coerce.Get(body, "valor")),
                Coerce.AsString(Coerce.Get(body, "data")),
                Coerce.AsInt(Coerce.Get(body, "tipo"), 0),
                Coerce.AsBool(Coerce.Get(body, "pago")) ?? false,
                Coerce.AsString(Coerce.Get(body, "data_pagamento")),
                Coerce.AsString(Coerce.Get(body, "notas")));
            var newId = Convert.ToInt32(inserted[0]["id"]);
            var row = await conn.QueryFirstOrDefaultAsync($"{Select} WHERE id = $1", newId);
            return Results.Created($"/api/despesas/{newId}", row);
        });

        app.MapMethods("/api/despesas/{id:int}", new[] { "PATCH" }, async (int id, Dictionary<string, JsonElement>? body) =>
        {
            string[] keys = { "descricao", "valor", "data", "tipo", "pago", "data_pagamento", "notas" };
            var sets = new List<string>();
            var args = new List<object?>();
            foreach (var k in keys)
            {
                var v = Coerce.Get(body, k);
                if (v is null) continue;
                object? typed = k switch
                {
                    "valor" => Coerce.AsNum(v),
                    "tipo" => Coerce.AsInt(v),
                    "pago" => Coerce.AsBool(v),
                    _ => Coerce.AsString(v),
                };
                args.Add(typed);
                sets.Add($"{k} = ${args.Count}");
            }
            await using var conn = await Db.OpenAsync();
            if (sets.Count > 0)
            {
                args.Add(id);
                await conn.ExecuteAsync(
                    $"UPDATE despesas SET {string.Join(", ", sets)} WHERE id = ${args.Count}",
                    args.ToArray());
            }
            var row = await conn.QueryFirstOrDefaultAsync($"{Select} WHERE id = $1", id);
            return row is null
                ? Results.NotFound(new { error = "Despesa não encontrada" })
                : Results.Ok(row);
        });

        app.MapDelete("/api/despesas/{id:int}", async (int id) =>
        {
            await using var conn = await Db.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM despesas WHERE id = $1", id);
            return Results.NoContent();
        });
    }
}

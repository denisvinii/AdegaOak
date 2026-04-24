using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class PrecosEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/precos", async () =>
        {
            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(@"
SELECT productid, bebida, tamanho, material,
    valor::float8 AS valor,
    valor_venda::float8 AS valor_venda,
    valor_caixa::float8 AS valor_caixa,
    valor_atacado_caixa::float8 AS valor_atacado_caixa,
    quantidade_caixa,
    CASE WHEN valor > 0 THEN ((valor_venda - valor) / valor * 100)::float8 ELSE 0 END AS margem_percentual
FROM estoque
WHERE COALESCE(ativo, true) = true
ORDER BY bebida, tamanho");
            return Results.Ok(rows);
        });

        app.MapMethods("/api/precos", new[] { "PATCH" }, async (Dictionary<string, JsonElement>? body) =>
        {
            if (body is null || !body.TryGetValue("items", out var itemsEl) || itemsEl.ValueKind != JsonValueKind.Array)
                return Results.Ok(new { updated = 0 });

            await using var conn = await Db.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            int updated = 0;
            try
            {
                string[] keys = { "valor", "valor_venda", "valor_caixa", "valor_atacado_caixa", "quantidade_caixa" };
                foreach (var item in itemsEl.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    if (!item.TryGetProperty("productid", out var pidEl)) continue;
                    var pid = Coerce.AsInt(pidEl);
                    if (pid is null) continue;

                    var sets = new List<string>();
                    var args = new List<object?>();
                    foreach (var k in keys)
                    {
                        if (!item.TryGetProperty(k, out var v)) continue;
                        if (v.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) continue;
                        object? typed = k == "quantidade_caixa" ? Coerce.AsInt(v) : Coerce.AsNum(v);
                        args.Add(typed);
                        sets.Add($"{k} = ${args.Count}");
                    }
                    if (sets.Count == 0) continue;
                    args.Add(pid);
                    var n = await tx.ExecuteAsync(
                        $"UPDATE estoque SET {string.Join(", ", sets)} WHERE productid = ${args.Count}",
                        args.ToArray());
                    updated += n;
                }
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            return Results.Ok(new { updated });
        });
    }
}

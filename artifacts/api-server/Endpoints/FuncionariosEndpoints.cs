using System.Text.Json;
using AdegaOak.Api.Infra;

namespace AdegaOak.Api.Endpoints;

public static class FuncionariosEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/funcionarios", async () =>
        {
            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(
                "SELECT id, username, COALESCE(ativo, true) AS ativo FROM funcionarios " +
                "WHERE COALESCE(ativo, true) = true ORDER BY username");
            return Results.Ok(rows);
        });

        app.MapPost("/api/funcionarios", async (Dictionary<string, JsonElement>? body) =>
        {
            var username = (Coerce.AsString(Coerce.Get(body, "username")) ?? "").Trim();
            if (username.Length < 2) return Results.BadRequest(new { error = "username inválido" });
            await using var conn = await Db.OpenAsync();
            var rows = await conn.QueryAsync(
                "INSERT INTO funcionarios (username, ativo, criado_em) VALUES ($1, true, NOW()) " +
                "RETURNING id, username, ativo", username);
            return Results.Created($"/api/funcionarios/{rows[0]["id"]}", rows[0]);
        });

        app.MapDelete("/api/funcionarios/{id:int}", async (int id) =>
        {
            await using var conn = await Db.OpenAsync();
            await conn.ExecuteAsync("UPDATE funcionarios SET ativo = false WHERE id = $1", id);
            return Results.NoContent();
        });
    }
}

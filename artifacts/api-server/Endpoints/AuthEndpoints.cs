using System.Text.Json;

namespace AdegaOak.Api.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/verify-discount", (Dictionary<string, JsonElement>? body) =>
        {
            var expected = Environment.GetEnvironmentVariable("ADMIN_DESCONTO_SENHA") ?? "ADEGA2024";
            string given = "";
            if (body is not null && body.TryGetValue("password", out var je) && je.ValueKind == JsonValueKind.String)
                given = je.GetString() ?? "";
            return Results.Ok(new { valid = given.Length > 0 && given == expected });
        });
    }
}

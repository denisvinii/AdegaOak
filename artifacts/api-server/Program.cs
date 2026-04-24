using AdegaOak.Api.Endpoints;
using AdegaOak.Api.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
});

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = null;
    opts.SerializerOptions.DictionaryKeyPolicy = null;
});

var portStr = Environment.GetEnvironmentVariable("PORT")
    ?? throw new InvalidOperationException("PORT environment variable is required.");
if (!int.TryParse(portStr, out var port) || port <= 0)
    throw new InvalidOperationException($"Invalid PORT value: \"{portStr}\"");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Method == "OPTIONS")
    {
        ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
        ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PATCH,PUT,DELETE,OPTIONS";
        ctx.Response.Headers["Access-Control-Allow-Headers"] = "*";
        ctx.Response.StatusCode = 204;
        return;
    }
    ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
    await next();
});

app.MapGet("/api/healthz", () => Results.Ok(new { status = "ok" }));

EstoqueEndpoints.Map(app);
MovimentacoesEndpoints.Map(app);
PrecosEndpoints.Map(app);
CombosEndpoints.Map(app);
VendasEndpoints.Map(app);
DespesasEndpoints.Map(app);
FuncionariosEndpoints.Map(app);
AuthEndpoints.Map(app);
DashboardEndpoints.Map(app);

app.Logger.LogInformation("AdegaOak API listening on port {Port}", port);
app.Run();

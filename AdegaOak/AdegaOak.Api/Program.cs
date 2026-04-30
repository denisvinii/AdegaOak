using System.Text;
using AdegaOak.Data.Data;
using AdegaOak.Data.Repositories;using AdegaOak.Services.Interfaces;
using AdegaOak.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for Railway proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Railway provides PORT env var — bind to it (Railway uses 8080 internally)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Database — use absolute path for SQLite in Docker/Railway
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=adegaoak.db";

// If relative path, make it absolute based on app directory
if (connectionString.StartsWith("Data Source=") && !connectionString.Contains("/"))
{
    var dbFile = connectionString.Replace("Data Source=", "").Trim();
    var dbPath = Path.Combine("/app", dbFile);
    connectionString = $"Data Source={dbPath}";
}

builder.Services.AddDbContext<AdegaOakDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>();
builder.Services.AddScoped<IComboRepository, ComboRepository>();
builder.Services.AddScoped<ISaldoRepository, SaldoRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IMovimentacaoService, MovimentacaoService>();
builder.Services.AddScoped<IDespesaService, DespesaService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? builder.Configuration["JWT_KEY"]
    ?? throw new InvalidOperationException("JWT Key não configurada");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Response Compression for faster API responses
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// CORS - CRITICAL: Must be configured before controllers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOriginsEnv = Environment.GetEnvironmentVariable("AllowedOrigins")
            ?? builder.Configuration["AllowedOrigins"];

        if (!string.IsNullOrEmpty(allowedOriginsEnv))
        {
            var origins = allowedOriginsEnv.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"[CORS] Configured origins from env: {string.Join(", ", origins)}");
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed(_ => true); // Allow during preflight
        }
        else
        {
            Console.WriteLine("[CORS] No AllowedOrigins env var found, using fallback pattern matching");
            // Fallback: allow all vercel.app and localhost origins
            policy.SetIsOriginAllowed(origin =>
                {
                    var allowed = origin.EndsWith(".vercel.app") || 
                                  origin.StartsWith("http://localhost") ||
                                  origin.StartsWith("https://localhost");
                    Console.WriteLine($"[CORS] Origin check: {origin} -> {allowed}");
                    return allowed;
                })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AdegaOak API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Use forwarded headers from Railway proxy
app.UseForwardedHeaders();

// Migrate database only — no seeding.
// The admin user must be inserted directly in the database.
// See: Database/README_DATABASE.md for instructions.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdegaOakDbContext>();
    // Ensure directory exists for SQLite file
    var connStr = db.Database.GetConnectionString() ?? "";
    var dbPath = connStr.Replace("Data Source=", "").Trim();
    if (!string.IsNullOrEmpty(dbPath) && !Path.IsPathRooted(dbPath))
    {
        dbPath = Path.Combine(AppContext.BaseDirectory, dbPath);
    }
    var dir = Path.GetDirectoryName(dbPath);
    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
    {
        Directory.CreateDirectory(dir);
    }
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Also expose Swagger in production for Railway health checks
    app.UseSwagger();
}

// CRITICAL: CORS must be applied BEFORE Authentication/Authorization
app.UseCors("AllowFrontend");
app.UseResponseCompression(); // Enable compression
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("[STARTUP] Application started successfully");
Console.WriteLine($"[STARTUP] Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"[STARTUP] AllowedOrigins: {Environment.GetEnvironmentVariable("AllowedOrigins") ?? "Not set (using fallback)"}");

app.Run();

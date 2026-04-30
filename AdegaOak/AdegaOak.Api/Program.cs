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

// Database configuration - PostgreSQL (Supabase) only
Console.WriteLine("[DATABASE] Reading DATABASE_URL environment variable...");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"[DATABASE] DATABASE_URL from env: {(string.IsNullOrEmpty(databaseUrl) ? "NULL/EMPTY" : "EXISTS")}");

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    Console.WriteLine("[DATABASE] DATABASE_URL is empty, trying appsettings.json...");
    databaseUrl = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"[DATABASE] DefaultConnection from config: {(string.IsNullOrEmpty(databaseUrl) ? "NULL/EMPTY" : "EXISTS")}");
}

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    Console.WriteLine("[DATABASE] ❌ ERROR: DATABASE_URL not configured!");
    Console.WriteLine("[DATABASE] Please configure DATABASE_URL environment variable with your Supabase connection string.");
    Console.WriteLine("[DATABASE] Example: postgresql://postgres.xxxxx:password@aws-0-us-east-1.pooler.supabase.com:6543/postgres");
    
    // List all environment variables for debugging
    Console.WriteLine("[DATABASE] Available environment variables:");
    foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
    {
        if (env.Key.ToString().Contains("DATABASE", StringComparison.OrdinalIgnoreCase) ||
            env.Key.ToString().Contains("CONNECTION", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[DATABASE]   {env.Key} = {env.Value}");
        }
    }
    
    throw new InvalidOperationException(
        "DATABASE_URL environment variable is required. " +
        "Please configure it with your Supabase PostgreSQL connection string.");
}

// Debug: Show connection string length and first/last chars
Console.WriteLine($"[DATABASE] Connection string length: {databaseUrl.Length}");
Console.WriteLine($"[DATABASE] First 20 chars: {(databaseUrl.Length >= 20 ? databaseUrl.Substring(0, 20) : databaseUrl)}");
Console.WriteLine($"[DATABASE] Last 20 chars: {(databaseUrl.Length >= 20 ? databaseUrl.Substring(databaseUrl.Length - 20) : databaseUrl)}");
Console.WriteLine($"[DATABASE] Contains newline: {databaseUrl.Contains('\n')}");
Console.WriteLine($"[DATABASE] Contains carriage return: {databaseUrl.Contains('\r')}");
Console.WriteLine($"[DATABASE] Starts with space: {databaseUrl.StartsWith(" ")}");
Console.WriteLine($"[DATABASE] Ends with space: {databaseUrl.EndsWith(" ")}");

// Trim whitespace and remove any hidden characters
databaseUrl = databaseUrl.Trim().Replace("\r", "").Replace("\n", "");
Console.WriteLine($"[DATABASE] After trim length: {databaseUrl.Length}");

// Validate connection string format
if (!databaseUrl.StartsWith("postgresql://") && !databaseUrl.StartsWith("postgres://") && 
    !databaseUrl.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("[DATABASE] ❌ ERROR: Invalid connection string format!");
    Console.WriteLine($"[DATABASE] Current value: {databaseUrl}");
    Console.WriteLine("[DATABASE] Expected format: postgresql://user:password@host:port/database");
    Console.WriteLine("[DATABASE] Or: Host=host;Database=db;Username=user;Password=pass");
    throw new InvalidOperationException(
        "DATABASE_URL has invalid format. " +
        "Expected PostgreSQL connection string starting with 'postgresql://' or 'Host='");
}

// URL-encode special characters in password if using URI format
if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo;
        
        if (!string.IsNullOrEmpty(userInfo) && userInfo.Contains(':'))
        {
            var parts = userInfo.Split(':', 2);
            var username = parts[0];
            var password = parts[1];
            
            // Check if password contains special characters that need encoding
            if (password.Contains('#') || password.Contains('@') || password.Contains('&') || 
                password.Contains('=') || password.Contains('+') || password.Contains(' '))
            {
                Console.WriteLine("[DATABASE] ⚠️  Password contains special characters, URL-encoding...");
                var encodedPassword = Uri.EscapeDataString(password);
                
                // Rebuild connection string with encoded password
                var scheme = uri.Scheme;
                var host = uri.Host;
                var port = uri.Port;
                var database = uri.AbsolutePath.TrimStart('/');
                
                databaseUrl = $"{scheme}://{username}:{encodedPassword}@{host}:{port}/{database}";
                Console.WriteLine("[DATABASE] ✅ Password encoded successfully");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DATABASE] ⚠️  Could not parse URI for password encoding: {ex.Message}");
        Console.WriteLine("[DATABASE] Proceeding with original connection string...");
    }
}

Console.WriteLine("[DATABASE] Using PostgreSQL (Supabase)");
Console.WriteLine($"[DATABASE] Connection: {MaskConnectionString(databaseUrl)}");

// Test connection string parsing before registering DbContext
try
{
    Console.WriteLine("[DATABASE] Testing connection string parsing...");
    Console.WriteLine($"[DATABASE] Full connection string (masked): {MaskConnectionString(databaseUrl)}");
    Console.WriteLine($"[DATABASE] String is null or empty: {string.IsNullOrEmpty(databaseUrl)}");
    Console.WriteLine($"[DATABASE] String is whitespace: {string.IsNullOrWhiteSpace(databaseUrl)}");
    
    // Try to parse with NpgsqlConnectionStringBuilder
    var testBuilder = new Npgsql.NpgsqlConnectionStringBuilder(databaseUrl);
    Console.WriteLine($"[DATABASE] ✅ Connection string parsed successfully");
    Console.WriteLine($"[DATABASE] Host: {testBuilder.Host}");
    Console.WriteLine($"[DATABASE] Port: {testBuilder.Port}");
    Console.WriteLine($"[DATABASE] Database: {testBuilder.Database}");
    Console.WriteLine($"[DATABASE] Username: {testBuilder.Username}");
    Console.WriteLine($"[DATABASE] Password set: {!string.IsNullOrEmpty(testBuilder.Password)}");
}
catch (Exception ex)
{
    Console.WriteLine($"[DATABASE] ❌ ERROR parsing connection string!");
    Console.WriteLine($"[DATABASE] Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"[DATABASE] Exception Message: {ex.Message}");
    Console.WriteLine($"[DATABASE] Connection string length: {databaseUrl?.Length ?? 0}");
    
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine($"[DATABASE] First 50 chars: {(databaseUrl.Length >= 50 ? databaseUrl.Substring(0, 50) : databaseUrl)}");
        Console.WriteLine($"[DATABASE] Contains special chars: # = {databaseUrl.Contains('#')}, @ = {databaseUrl.Contains('@')}, & = {databaseUrl.Contains('&')}");
    }
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[DATABASE] Inner Exception: {ex.InnerException.Message}");
    }
    
    Console.WriteLine($"[DATABASE] Stack Trace: {ex.StackTrace}");
    
    throw new InvalidOperationException(
        $"Failed to parse DATABASE_URL connection string: {ex.Message}. " +
        "Please verify the connection string format is correct: postgresql://user:password@host:port/database", ex);
}

// Register DbContext with validated connection string
Console.WriteLine("[DATABASE] Registering DbContext with Entity Framework...");
builder.Services.AddDbContext<AdegaOakDbContext>(options =>
{
    Console.WriteLine("[DATABASE] DbContext factory called, using connection string");
    options.UseNpgsql(databaseUrl);
});

// Helper function to mask sensitive connection string data
static string MaskConnectionString(string connStr)
{
    if (string.IsNullOrEmpty(connStr)) return "null";
    
    var parts = connStr.Split(';');
    var masked = new List<string>();
    
    foreach (var part in parts)
    {
        if (part.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            masked.Add("Password=***");
        }
        else if (part.Contains("://") && part.Contains("@"))
        {
            // Mask password in URL format: postgres://user:password@host
            var atIndex = part.IndexOf('@');
            var colonIndex = part.LastIndexOf(':', atIndex);
            if (colonIndex > 0)
            {
                masked.Add(part.Substring(0, colonIndex + 1) + "***" + part.Substring(atIndex));
            }
            else
            {
                masked.Add(part);
            }
        }
        else
        {
            masked.Add(part);
        }
    }
    
    return string.Join(";", masked);
}

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

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdegaOakDbContext>();
    
    try
    {
        Console.WriteLine("[DATABASE] Applying migrations...");
        db.Database.Migrate();
        Console.WriteLine("[DATABASE] ✅ Migrations applied successfully");
        
        // Verify database connection
        var canConnect = db.Database.CanConnect();
        Console.WriteLine($"[DATABASE] Connection verified: {canConnect}");
        
        // Check if admin user exists
        var usuariosCount = db.Usuarios.Count();
        Console.WriteLine($"[DATABASE] ✅ Users in database: {usuariosCount}");
        
        if (usuariosCount == 0)
        {
            Console.WriteLine("[DATABASE] ⚠️  WARNING: No users found!");
            Console.WriteLine("[DATABASE] Make sure migrations include seed data for admin user.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DATABASE] ❌ ERROR: {ex.Message}");
        Console.WriteLine($"[DATABASE] Type: {ex.GetType().Name}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[DATABASE] Inner: {ex.InnerException.Message}");
        }
        
        throw;
    }
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

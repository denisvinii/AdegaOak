using System.Text;
using AdegaOak.Data.Data;
using AdegaOak.Data.Repositories;
using AdegaOak.Services.Interfaces;
using AdegaOak.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/adegaoak-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting AdegaOak API");

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

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
Log.Information("[DATABASE] Reading DATABASE_URL environment variable");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Log.Information("[DATABASE] DATABASE_URL from env: {Status}", string.IsNullOrEmpty(databaseUrl) ? "NULL/EMPTY" : "EXISTS");

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    Log.Warning("[DATABASE] DATABASE_URL is empty, trying appsettings.json");
    databaseUrl = builder.Configuration.GetConnectionString("DefaultConnection");
    Log.Information("[DATABASE] DefaultConnection from config: {Status}", string.IsNullOrEmpty(databaseUrl) ? "NULL/EMPTY" : "EXISTS");
}

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    Log.Fatal("[DATABASE] DATABASE_URL not configured!");
    Log.Fatal("[DATABASE] Please configure DATABASE_URL environment variable with your Supabase connection string");
    Log.Fatal("[DATABASE] Example: postgresql://postgres.xxxxx:password@aws-0-us-east-1.pooler.supabase.com:6543/postgres");
    
    throw new InvalidOperationException(
        "DATABASE_URL environment variable is required. " +
        "Please configure it with your Supabase PostgreSQL connection string.");
}

// Trim whitespace and remove any hidden characters
databaseUrl = databaseUrl.Trim().Replace("\r", "").Replace("\n", "");

// Validate connection string format
if (!databaseUrl.StartsWith("postgresql://") && !databaseUrl.StartsWith("postgres://") && 
    !databaseUrl.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
{
    Log.Fatal("[DATABASE] Invalid connection string format");
    Log.Fatal("[DATABASE] Expected format: postgresql://user:password@host:port/database");
    throw new InvalidOperationException(
        "DATABASE_URL has invalid format. " +
        "Expected PostgreSQL connection string starting with 'postgresql://' or 'Host='");
}

Log.Information("[DATABASE] Using PostgreSQL (Supabase)");

// Convert URI format to Npgsql connection string format
string npgsqlConnectionString = databaseUrl;

if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
{
    try
    {
        Log.Information("[DATABASE] Converting URI format to Npgsql format");
        var uri = new Uri(databaseUrl);
        
        var host = uri.Host;
        var dbPort = uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        
        // Build Npgsql-compatible connection string with robust settings for Supabase
        var useSessionPooler = dbPort == 6543;
        var finalPort = useSessionPooler ? 5432 : dbPort;
        
        npgsqlConnectionString = $"Host={host};Port={finalPort};Database={database};Username={username};Password={password};SSL Mode=Require;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;Connection Idle Lifetime=300;Connection Pruning Interval=10;Timeout=60;Command Timeout=60;Keepalive=30;TCP Keepalive=true;TCP Keepalive Time=30;TCP Keepalive Interval=10";
        
        if (useSessionPooler)
        {
            Log.Warning("[DATABASE] Detected Transaction Pooler (port 6543), switching to Session Pooler (port 5432) for better stability");
        }
        
        Log.Information("[DATABASE] Converted to Npgsql format successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "[DATABASE] Could not convert URI format, will try to use original format");
    }
}

// Test connection string parsing before registering DbContext
try
{
    Log.Information("[DATABASE] Testing connection string parsing");
    var testBuilder = new Npgsql.NpgsqlConnectionStringBuilder(npgsqlConnectionString);
    Log.Information("[DATABASE] Connection string parsed successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "[DATABASE] Failed to parse connection string");
    throw new InvalidOperationException(
        $"Failed to parse DATABASE_URL connection string: {ex.Message}. " +
        "Please verify the connection string format is correct: postgresql://user:password@host:port/database", ex);
}

// Register DbContext with validated connection string
Log.Information("[DATABASE] Registering DbContext with Entity Framework");
builder.Services.AddDbContext<AdegaOakDbContext>(options =>
{
    options.UseNpgsql(npgsqlConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
        npgsqlOptions.CommandTimeout(60);
    });
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

// Memory Cache (necessário para ProdutoService)
builder.Services.AddMemoryCache();

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
builder.Services.AddScoped<IVendaService, VendaService>();

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
        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin))
                {
                    return false;
                }

                var allowed = origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase) || 
                              origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                              origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase);
                
                Log.Debug("[CORS] Origin: {Origin} -> {Status}", origin, allowed ? "ALLOWED" : "DENIED");
                return allowed;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
            .WithExposedHeaders("*");
        
        Log.Information("[CORS] Policy configured: Allow all *.vercel.app and localhost origins");
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

// CRITICAL: Configure CORS BEFORE trying migrations
// This ensures CORS headers are sent even if database fails
app.UseCors("AllowFrontend");

// Add middleware to ensure CORS headers are always present, even on errors
app.Use(async (context, next) =>
{
    // Add CORS headers manually as a fallback
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin) && 
        (origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase) || 
         origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
         origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        context.Response.Headers.Append("Access-Control-Expose-Headers", "*");
        context.Response.Headers.Append("Access-Control-Max-Age", "600");
    }
    
    // Handle preflight requests
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    
    await next();
});

// Apply migrations automatically on startup
// DISABLED: Tables already created manually in Supabase
/*
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
        
        Console.WriteLine("[DATABASE] ⚠️  WARNING: Application will start but database operations will fail!");
        Console.WriteLine("[DATABASE] Please check your DATABASE_URL and Supabase connection.");
        // Don't throw - let the app start so we can see CORS errors properly
    }
}
*/

Log.Information("[DATABASE] Migrations disabled - using existing Supabase tables");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
}

// CORS is already applied above (before migrations)
app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("[STARTUP] Application started successfully");
Log.Information("[STARTUP] Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

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

// Database — use volume path if available, otherwise use app directory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=adegaoak.db";

// Check if running in Railway with volume
var volumePath = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                  Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") != null;

if (connectionString.StartsWith("Data Source=") && !connectionString.Contains("/"))
{
    var dbFile = connectionString.Replace("Data Source=", "").Trim();
    
    if (!string.IsNullOrEmpty(volumePath))
    {
        // Use Railway volume for persistence
        var dbPath = Path.Combine(volumePath, dbFile);
        connectionString = $"Data Source={dbPath}";
        Console.WriteLine($"[DATABASE] Using Railway volume: {dbPath}");
    }
    else if (isContainer)
    {
        // Use /app directory in container (data will NOT persist between deploys)
        var dbPath = Path.Combine("/app", dbFile);
        connectionString = $"Data Source={dbPath}";
        Console.WriteLine($"[DATABASE] Container mode (NO PERSISTENCE): {dbPath}");
        Console.WriteLine($"[DATABASE] ⚠️  WARNING: Data will be lost on redeploy!");
        Console.WriteLine($"[DATABASE] Configure a Railway Volume for data persistence.");
    }
    else
    {
        // Use relative path in development
        Console.WriteLine($"[DATABASE] Development mode: {dbFile}");
    }
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

// Migrate database and seed admin user if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdegaOakDbContext>();
    
    try
    {
        // Ensure directory exists for SQLite file
        var connStr = db.Database.GetConnectionString() ?? "";
        var dbPath = connStr.Replace("Data Source=", "").Trim();
        
        Console.WriteLine($"[DATABASE] Connection string: {connStr}");
        Console.WriteLine($"[DATABASE] Database path: {dbPath}");
        
        if (!string.IsNullOrEmpty(dbPath) && !Path.IsPathRooted(dbPath))
        {
            dbPath = Path.Combine(AppContext.BaseDirectory, dbPath);
            Console.WriteLine($"[DATABASE] Resolved path: {dbPath}");
        }
        
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Console.WriteLine($"[DATABASE] Creating directory: {dir}");
            Directory.CreateDirectory(dir);
        }
        
        // Check if database file exists
        var dbFileExists = File.Exists(dbPath);
        Console.WriteLine($"[DATABASE] Database file exists: {dbFileExists}");
        
        // Apply migrations - this will create the database and tables
        Console.WriteLine("[DATABASE] Applying migrations...");
        try
        {
            db.Database.Migrate();
            Console.WriteLine("[DATABASE] ✅ Migrations applied successfully");
        }
        catch (Exception migrationEx)
        {
            Console.WriteLine($"[DATABASE] ⚠️  Migration failed: {migrationEx.Message}");
            Console.WriteLine("[DATABASE] Attempting to create database with EnsureCreated...");
            db.Database.EnsureCreated();
            Console.WriteLine("[DATABASE] ✅ Database created with EnsureCreated");
        }
        
        // Verify database is accessible
        Console.WriteLine("[DATABASE] Verifying database connection...");
        var canConnect = db.Database.CanConnect();
        Console.WriteLine($"[DATABASE] Can connect: {canConnect}");
        
        if (!canConnect)
        {
            throw new Exception("Cannot connect to database after migration");
        }
        
        // Verify Usuarios table exists and check count
        Console.WriteLine("[DATABASE] Checking if Usuarios table exists...");
        int usuariosCount = 0;
        try
        {
            usuariosCount = db.Usuarios.Count();
            Console.WriteLine($"[DATABASE] ✅ Usuarios table exists. Count: {usuariosCount}");
            
            if (usuariosCount > 0)
            {
                Console.WriteLine("[DATABASE] ✅ Database initialized successfully with seed data");
            }
            else
            {
                Console.WriteLine("[DATABASE] ⚠️  WARNING: No users found in database!");
                Console.WriteLine("[DATABASE] The migration should have created an admin user.");
                Console.WriteLine("[DATABASE] Check if migrations were applied correctly.");
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
        {
            Console.WriteLine($"[DATABASE] ⚠️  Usuarios table does not exist! Error: {ex.Message}");
            Console.WriteLine("[DATABASE] Migration may have failed. Attempting to create database schema...");
            
            try
            {
                // Force database creation with migrations
                db.Database.EnsureDeleted();
                db.Database.Migrate();
                Console.WriteLine("[DATABASE] ✅ Database schema created successfully with migrations");
                
                // Verify table now exists and has seed data
                usuariosCount = db.Usuarios.Count();
                Console.WriteLine($"[DATABASE] Usuarios count after migration: {usuariosCount}");
                
                if (usuariosCount > 0)
                {
                    Console.WriteLine("[DATABASE] ✅ Admin user created by migration");
                }
            }
            catch (Exception createEx)
            {
                Console.WriteLine($"[DATABASE] ❌ Failed to create database: {createEx.Message}");
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DATABASE] ❌ FATAL ERROR: {ex.Message}");
        Console.WriteLine($"[DATABASE] Exception type: {ex.GetType().Name}");
        Console.WriteLine($"[DATABASE] Stack trace: {ex.StackTrace}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[DATABASE] Inner exception: {ex.InnerException.Message}");
            Console.WriteLine($"[DATABASE] Inner stack trace: {ex.InnerException.StackTrace}");
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

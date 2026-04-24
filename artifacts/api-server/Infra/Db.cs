using Npgsql;

namespace AdegaOak.Api.Infra;

public static class Db
{
    private static readonly string ConnectionString = BuildConnectionString();

    private static string BuildConnectionString()
    {
        var url = Environment.GetEnvironmentVariable("SUPABASE_DATABASE_URL")
                  ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                  ?? throw new InvalidOperationException(
                      "SUPABASE_DATABASE_URL (or DATABASE_URL) environment variable is required.");

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pwd = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var db = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        var b = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = port,
            Username = user,
            Password = pwd,
            Database = db,
            SslMode = SslMode.Require,
            Pooling = true,
            MaxPoolSize = 10,
            MinPoolSize = 0,
            Timeout = 15,
            CommandTimeout = 30,
            // Supabase's pooler drops idle TCP connections; keep them alive and
            // recycle the pool periodically so we don't try to reuse a dead socket.
            KeepAlive = 30,
            TcpKeepAlive = true,
            ConnectionIdleLifetime = 60,
            ConnectionPruningInterval = 30,
            MaxAutoPrepare = 0,
        };
        return b.ToString();
    }

    public static async Task<NpgsqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}

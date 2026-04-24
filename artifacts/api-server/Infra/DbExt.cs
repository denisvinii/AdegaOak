using Npgsql;

namespace AdegaOak.Api.Infra;

public static class DbExt
{
    /// <summary>
    /// Run a parameterized query (uses Postgres positional placeholders $1, $2, ...).
    /// Returns rows as ordered dictionaries with column names as-is (so SQL aliases
    /// like `valor::float8 AS valor_venda` produce snake_case JSON keys directly).
    /// </summary>
    public static async Task<List<Dictionary<string, object?>>> QueryAsync(
        this NpgsqlConnection conn, string sql, params object?[] args)
    {
        await using var cmd = BuildCommand(conn, sql, args, transaction: null);
        return await ReadAllAsync(cmd);
    }

    public static async Task<List<Dictionary<string, object?>>> QueryAsync(
        this NpgsqlTransaction tx, string sql, params object?[] args)
    {
        await using var cmd = BuildCommand(tx.Connection!, sql, args, transaction: tx);
        return await ReadAllAsync(cmd);
    }

    public static async Task<Dictionary<string, object?>?> QueryFirstOrDefaultAsync(
        this NpgsqlConnection conn, string sql, params object?[] args)
    {
        var rows = await conn.QueryAsync(sql, args);
        return rows.Count > 0 ? rows[0] : null;
    }

    public static async Task<Dictionary<string, object?>?> QueryFirstOrDefaultAsync(
        this NpgsqlTransaction tx, string sql, params object?[] args)
    {
        var rows = await tx.QueryAsync(sql, args);
        return rows.Count > 0 ? rows[0] : null;
    }

    public static async Task<int> ExecuteAsync(
        this NpgsqlConnection conn, string sql, params object?[] args)
    {
        await using var cmd = BuildCommand(conn, sql, args, transaction: null);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> ExecuteAsync(
        this NpgsqlTransaction tx, string sql, params object?[] args)
    {
        await using var cmd = BuildCommand(tx.Connection!, sql, args, transaction: tx);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<T?> ExecuteScalarAsync<T>(
        this NpgsqlConnection conn, string sql, params object?[] args)
    {
        await using var cmd = BuildCommand(conn, sql, args, transaction: null);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? default : (T)Convert.ChangeType(v, typeof(T));
    }

    private static NpgsqlCommand BuildCommand(
        NpgsqlConnection conn, string sql, object?[] args, NpgsqlTransaction? transaction)
    {
        var cmd = new NpgsqlCommand(sql, conn);
        if (transaction is not null) cmd.Transaction = transaction;
        foreach (var arg in args)
        {
            cmd.Parameters.Add(new NpgsqlParameter { Value = arg ?? DBNull.Value });
        }
        return cmd;
    }

    private static async Task<List<Dictionary<string, object?>>> ReadAllAsync(NpgsqlCommand cmd)
    {
        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(reader.FieldCount, StringComparer.Ordinal);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }
        return rows;
    }
}

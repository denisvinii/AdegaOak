namespace AdegaOak.Models.Extensions;

/// <summary>
/// Extensões para trabalhar com DateTime e PostgreSQL
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converte DateTime para UTC, corrigindo o Kind se necessário.
    /// PostgreSQL exige DateTime com Kind=Utc.
    /// </summary>
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => dateTime
        };
    }

    /// <summary>
    /// Converte DateTime nullable para UTC
    /// </summary>
    public static DateTime? ToUtc(this DateTime? dateTime)
    {
        return dateTime?.ToUtc();
    }
}

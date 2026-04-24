using System.Globalization;
using System.Text.Json;

namespace AdegaOak.Api.Infra;

/// <summary>
/// Helpers to coerce values coming from JSON request bodies (which arrive as JsonElement)
/// or query strings (strings) into typed primitives, mirroring the asInt/asNum/asBool
/// helpers from the original TypeScript backend.
/// </summary>
public static class Coerce
{
    public static int? AsInt(object? v, int? fallback = null)
    {
        if (v is null) return fallback;
        if (v is JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined: return fallback;
                case JsonValueKind.Number:
                    return je.TryGetInt32(out var n) ? n : (int)je.GetDouble();
                case JsonValueKind.String:
                    return AsInt(je.GetString(), fallback);
                default: return fallback;
            }
        }
        if (v is string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return fallback;
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? (int)d : fallback;
        }
        try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); }
        catch { return fallback; }
    }

    public static double? AsNum(object? v, double? fallback = null)
    {
        if (v is null) return fallback;
        if (v is JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined: return fallback;
                case JsonValueKind.Number: return je.GetDouble();
                case JsonValueKind.String:
                    var s = je.GetString();
                    if (string.IsNullOrWhiteSpace(s)) return fallback;
                    return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : fallback;
                default: return fallback;
            }
        }
        if (v is string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return fallback;
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : fallback;
        }
        try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
        catch { return fallback; }
    }

    public static bool? AsBool(object? v)
    {
        if (v is null) return null;
        if (v is bool b) return b;
        if (v is JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined: return null;
                case JsonValueKind.String: return AsBool(je.GetString());
                case JsonValueKind.Number: return je.GetDouble() != 0;
                default: return null;
            }
        }
        var s = v.ToString()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(s)) return null;
        if (s is "true" or "1" or "yes" or "sim") return true;
        if (s is "false" or "0" or "no" or "nao" or "não") return false;
        return null;
    }

    public static string? AsString(object? v)
    {
        if (v is null) return null;
        if (v is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => je.ToString()
            };
        }
        return v.ToString();
    }

    /// <summary>Get a property from a JsonElement-backed dictionary as object?.</summary>
    public static object? Get(Dictionary<string, JsonElement>? body, string key)
    {
        if (body is null || !body.TryGetValue(key, out var je)) return null;
        return je.ValueKind == JsonValueKind.Null || je.ValueKind == JsonValueKind.Undefined ? null : (object)je;
    }
}

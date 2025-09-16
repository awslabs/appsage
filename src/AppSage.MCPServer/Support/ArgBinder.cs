using System.Text.Json;

namespace AppSage.McpServer.Support;

public static class ArgBinder
{
    public static string MapTypeToJsonType(Type t)
    {
        if (t == typeof(string)) return "string";
        if (t == typeof(int) || t == typeof(long)) return "integer";
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return "number";
        if (t == typeof(bool)) return "boolean";
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string)) return "array";
        return "object";
    }

    public static object? ConvertArg(object? raw, Type target)
    {
        if (raw is null) return null;

        if (raw is JsonElement je)
        {
            // Handle null literal
            if (je.ValueKind == JsonValueKind.Null) return null;

            if (target == typeof(string)) return je.GetString();
            if (target == typeof(int)) return je.GetInt32();
            if (target == typeof(long)) return je.GetInt64();
            if (target == typeof(double)) return je.GetDouble();
            if (target == typeof(bool)) return je.GetBoolean();

            // Fallback: deserialize
            return je.Deserialize(target);
        }

        // Already concrete type
        if (target.IsInstanceOfType(raw)) return raw;

        return System.Text.Json.JsonSerializer.Deserialize(
            System.Text.Json.JsonSerializer.Serialize(raw), target);
    }
}

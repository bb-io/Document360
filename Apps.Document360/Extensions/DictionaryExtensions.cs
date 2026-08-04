namespace Apps.Document360.Extensions;

/// <summary>Small helpers that keep request-body building free of walls of null checks.</summary>
public static class DictionaryExtensions
{
    /// <summary>Adds the value when it is not null. An empty string is still sent.</summary>
    public static Dictionary<string, object> AddIfNotNull(this Dictionary<string, object> body, string key,
        string? value)
    {
        if (value is not null) body[key] = value;
        return body;
    }

    /// <summary>Adds the value only when it holds actual text.</summary>
    public static Dictionary<string, object> AddIfNotEmpty(this Dictionary<string, object> body, string key,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) body[key] = value;
        return body;
    }

    public static Dictionary<string, object> AddIfNotNull(this Dictionary<string, object> body, string key,
        bool? value)
    {
        if (value.HasValue) body[key] = value.Value;
        return body;
    }

    public static Dictionary<string, object> AddIfNotNull(this Dictionary<string, object> body, string key,
        int? value)
    {
        if (value.HasValue) body[key] = value.Value;
        return body;
    }
}

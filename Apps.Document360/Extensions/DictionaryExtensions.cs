namespace Apps.Document360.Extensions;


public static class DictionaryExtensions
{

    public static Dictionary<string, object> AddIfNotNull(this Dictionary<string, object> body, string key,
        string? value)
    {
        if (value is not null) body[key] = value;
        return body;
    }


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

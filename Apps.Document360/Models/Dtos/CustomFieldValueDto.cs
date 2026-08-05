using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

public class CustomFieldValueDto
{
    [JsonProperty("field_id")] public string FieldId { get; set; } = string.Empty;

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("type")] public int? Type { get; set; }

    [JsonProperty("value")] public string? Value { get; set; }

    [JsonProperty("options")] public List<CustomFieldOptionDto>? Options { get; set; }
}

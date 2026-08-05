using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class CustomFieldDefinitionDto
{
    [JsonProperty("field_id")] public string FieldId { get; set; } = string.Empty;

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("type")] public int? Type { get; set; }

    [JsonProperty("placeholder")] public string? Placeholder { get; set; }

    [JsonProperty("help_text")] public string? HelpText { get; set; }

    [JsonProperty("max_length")] public int? MaxLength { get; set; }

    [JsonProperty("options")] public List<CustomFieldOptionDto>? Options { get; set; }
}

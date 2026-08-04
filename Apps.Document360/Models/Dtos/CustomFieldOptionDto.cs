using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

public class CustomFieldOptionDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("label")] public string? Label { get; set; }
}

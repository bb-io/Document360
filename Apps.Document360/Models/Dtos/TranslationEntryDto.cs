using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class TranslationEntryDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("category_id")] public string? CategoryId { get; set; }
}

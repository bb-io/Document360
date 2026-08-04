using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// An entry of <c>GET /v2/Translations/{projectVersionId}/{langCode}</c>. It carries only identifiers, so it
/// serves as a set filter rather than as a source of article data.
/// </summary>
public class TranslationEntryDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("category_id")] public string? CategoryId { get; set; }
}

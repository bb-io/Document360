using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

public class AvailableLanguageDto
{
    [JsonProperty("lang_code")] public string LangCode { get; set; } = string.Empty;

    [JsonProperty("url")] public string? Url { get; set; }

    [JsonProperty("translation_status")] public int? TranslationStatus { get; set; }
}

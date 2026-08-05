using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class WorkspaceLocaleDto
{
    [JsonProperty("language_id")] public string LanguageId { get; set; } = string.Empty;

    [JsonProperty("language_code")] public string LanguageCode { get; set; } = string.Empty;

    [JsonProperty("language_name")] public string? LanguageName { get; set; }

    [JsonProperty("is_set_as_default")] public bool? IsSetAsDefault { get; set; }

    [JsonProperty("country_flag_code")] public string? CountryFlagCode { get; set; }
}

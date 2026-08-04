using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>A locale enabled on a workspace, as nested in the workspace list response.</summary>
public class ProjectVersionLanguageDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("code")] public string Code { get; set; } = string.Empty;

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("set_as_default")] public bool? SetAsDefault { get; set; }

    [JsonProperty("hidden")] public bool? Hidden { get; set; }

    [JsonProperty("enable_rtl")] public bool? EnableRtl { get; set; }

    [JsonProperty("site_protection_level")] public int? SiteProtectionLevel { get; set; }

    [JsonProperty("country_flag_code")] public string? CountryFlagCode { get; set; }
}

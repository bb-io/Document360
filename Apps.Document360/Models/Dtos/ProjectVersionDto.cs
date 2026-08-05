using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class ProjectVersionDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("version_code_name")] public string? VersionCodeName { get; set; }

    [JsonProperty("slug")] public string? Slug { get; set; }

    [JsonProperty("version_number")] public double? VersionNumber { get; set; }

    [JsonProperty("base_version_number")] public double? BaseVersionNumber { get; set; }

    [JsonProperty("is_main_version")] public bool? IsMainVersion { get; set; }

    [JsonProperty("is_beta")] public bool? IsBeta { get; set; }

    [JsonProperty("is_public")] public bool? IsPublic { get; set; }

    [JsonProperty("is_deprecated")] public bool? IsDeprecated { get; set; }

    [JsonProperty("version_type")] public int? VersionType { get; set; }

    [JsonProperty("order")] public int? Order { get; set; }

    [JsonProperty("created_at")] public DateTime? CreatedAt { get; set; }

    [JsonProperty("modified_at")] public DateTime? ModifiedAt { get; set; }

    [JsonProperty("language_versions")] public List<ProjectVersionLanguageDto>? LanguageVersions { get; set; }
}

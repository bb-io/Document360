using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// One version of an article translation. Returned by <c>GET /v2/Articles/{id}/{lang}/versions</c> and by the
/// fork endpoint (which returns the newly created version).
/// </summary>
public class ArticleVersionDto
{
    [JsonProperty("version_number")] public int VersionNumber { get; set; }

    [JsonProperty("base_version")] public int? BaseVersion { get; set; }

    [JsonProperty("status")] public int? Status { get; set; }

    [JsonProperty("created_by")] public string? CreatedBy { get; set; }

    [JsonProperty("comment")] public string? Comment { get; set; }

    [JsonProperty("created_at")] public DateTime? CreatedAt { get; set; }

    [JsonProperty("modified_at")] public DateTime? ModifiedAt { get; set; }
}

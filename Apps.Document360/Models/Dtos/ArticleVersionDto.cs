using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


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

using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class CreatedArticleDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("title")] public string? Title { get; set; }

    [JsonProperty("slug")] public string? Slug { get; set; }

    [JsonProperty("language_code")] public string? LanguageCode { get; set; }

    [JsonProperty("status")] public int? Status { get; set; }

    [JsonProperty("content_type")] public int? ContentType { get; set; }

    [JsonProperty("order")] public int? Order { get; set; }

    [JsonProperty("public_version")] public int? PublicVersion { get; set; }

    [JsonProperty("latest_version")] public int? LatestVersion { get; set; }
}

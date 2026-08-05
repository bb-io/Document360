using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class ArticleListItemDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("title")] public string? Title { get; set; }

    [JsonProperty("slug")] public string? Slug { get; set; }

    [JsonProperty("url")] public string? Url { get; set; }

    [JsonProperty("language_code")] public string? LanguageCode { get; set; }

    [JsonProperty("status")] public int? Status { get; set; }

    [JsonProperty("content_type")] public int? ContentType { get; set; }

    [JsonProperty("translation_option")] public int? TranslationOption { get; set; }

    [JsonProperty("security_visibility")] public int? SecurityVisibility { get; set; }

    [JsonProperty("public_version")] public int? PublicVersion { get; set; }

    [JsonProperty("latest_version")] public int? LatestVersion { get; set; }

    [JsonProperty("order")] public int? Order { get; set; }

    [JsonProperty("hidden")] public bool? Hidden { get; set; }

    [JsonProperty("is_shared_article")] public bool? IsSharedArticle { get; set; }

    [JsonProperty("exclude_from_external_search")]
    public bool? ExcludeFromExternalSearch { get; set; }

    [JsonProperty("current_workflow_status_id")]
    public string? CurrentWorkflowStatusId { get; set; }

    [JsonProperty("created_at")] public DateTime? CreatedAt { get; set; }

    [JsonProperty("modified_at")] public DateTime? ModifiedAt { get; set; }
}

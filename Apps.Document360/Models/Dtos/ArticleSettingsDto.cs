using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// <c>GET /v2/Articles/{articleId}/{langCode}/settings</c>. Tags live only here, and this is the response that
/// returns round-trippable custom field values (option IDs rather than labels).
/// </summary>
public class ArticleSettingsDto
{
    [JsonProperty("slug")] public string? Slug { get; set; }

    [JsonProperty("seo_title")] public string? SeoTitle { get; set; }

    /// <summary>The SEO meta description — a different field from the article description.</summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("tags")] public List<string>? Tags { get; set; }

    [JsonProperty("allow_comments")] public bool? AllowComments { get; set; }

    [JsonProperty("show_table_of_contents")] public bool? ShowTableOfContents { get; set; }

    [JsonProperty("exclude_from_search")] public bool? ExcludeFromSearch { get; set; }

    [JsonProperty("exclude_from_ai_search")] public bool? ExcludeFromAiSearch { get; set; }

    [JsonProperty("exclude_from_external_search")]
    public bool? ExcludeFromExternalSearch { get; set; }

    [JsonProperty("custom_fields")] public List<CustomFieldValueDto>? CustomFields { get; set; }
}

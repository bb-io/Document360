using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// A category node from <c>GET /v2/ProjectVersions/{projectVersionId}/categories</c>. The response is a tree;
/// <c>child_categories</c> has to be walked recursively.
/// </summary>
public class CategoryDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("slug")] public string? Slug { get; set; }

    [JsonProperty("parent_category_id")] public string? ParentCategoryId { get; set; }

    [JsonProperty("category_type")] public int? CategoryType { get; set; }

    [JsonProperty("hidden")] public bool? Hidden { get; set; }

    [JsonProperty("order")] public int? Order { get; set; }

    [JsonProperty("child_categories")] public List<CategoryDto>? ChildCategories { get; set; }
}

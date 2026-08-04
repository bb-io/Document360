using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

public class ArticleAuthorDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("first_name")] public string? FirstName { get; set; }

    [JsonProperty("last_name")] public string? LastName { get; set; }

    [JsonProperty("email_id")] public string? EmailId { get; set; }

    [JsonProperty("unique_user_name")] public string? UniqueUserName { get; set; }
}

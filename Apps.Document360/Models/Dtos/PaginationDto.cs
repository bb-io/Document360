using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

public class PaginationDto
{
    [JsonProperty("page")] public int Page { get; set; }

    [JsonProperty("hits_per_page")] public int HitsPerPage { get; set; }

    [JsonProperty("has_more")] public bool HasMore { get; set; }

    [JsonProperty("result_count")] public int ResultCount { get; set; }
}

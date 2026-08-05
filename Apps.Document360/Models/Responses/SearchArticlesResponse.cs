using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class SearchArticlesResponse
{
    [Display("Articles")] public List<ArticleListItemResponse> Articles { get; set; } = new();

    [Display("Total count", Description = "The number of articles returned.")]
    public int TotalCount { get; set; }
}

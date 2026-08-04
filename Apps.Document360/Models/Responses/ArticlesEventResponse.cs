using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class ArticlesEventResponse
{
    [Display("Articles")] public List<ArticleListItemResponse> Articles { get; set; } = new();

    [Display("Total count", Description = "The number of articles in this event.")]
    public int TotalCount { get; set; }
}

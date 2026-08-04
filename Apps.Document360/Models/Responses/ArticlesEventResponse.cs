using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Responses;


public class ArticlesEventResponse : IMultiDownloadableContentOutput<ArticleListItemResponse>
{
    [Display("Articles")] public List<ArticleListItemResponse> Items { get; set; } = [];

    [Display("Total count", Description = "The number of articles in this event.")]
    public int TotalCount { get; set; }
}

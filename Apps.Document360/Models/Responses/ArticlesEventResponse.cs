using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Responses;

/// <summary>
/// The articles one poll picked up. Implementing the blueprint's collection contract is what lets a workflow built
/// on another CMS be pointed at Document360 without being rebuilt.
/// </summary>
public class ArticlesEventResponse : IMultiDownloadableContentOutput<ArticleListItemResponse>
{
    [Display("Articles")] public List<ArticleListItemResponse> Items { get; set; } = [];

    [Display("Total count", Description = "The number of articles in this event.")]
    public int TotalCount { get; set; }
}

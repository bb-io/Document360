using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class PublishArticleResponse
{
    [Display("Article URL", Description = "The public address of the published translation.")]
    public string ArticleUrl { get; set; } = string.Empty;

    [Display("Published version", Description = "The version that is now live on the site.")]
    public int PublishedVersion { get; set; }

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Was already live",
        Description = "Turned on when this version was already live, so publishing changed nothing.")]
    public bool WasAlreadyLive { get; set; }
}

using Apps.Document360.Handlers;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Requests;

public class PublishArticleOptionalRequest : ArticleOptionalIdentifier
{
    [Display("User ID", Description = "The team account credited with publishing the article.")]
    [DataSource(typeof(TeamAccountDataHandler))]
    public string UserId { get; set; } = string.Empty;

    [Display("Version number",
        Description = "An older version to put live. Left empty, the newest version is published.")]
    [DataSource(typeof(ArticleVersionDataHandler))]
    public int? VersionNumber { get; set; }

    [Display("Publish message", Description = "A note recorded in the version history.")]
    public string? PublishMessage { get; set; }
}

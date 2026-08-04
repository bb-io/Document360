using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Requests;

public class GetArticleInfoRequest : ArticleIdentifier
{
    [Display("Use published version",
        Description = "Turn on to describe the version that is live on the site instead of the newest draft.")]
    public bool? UsePublishedVersion { get; set; }
}

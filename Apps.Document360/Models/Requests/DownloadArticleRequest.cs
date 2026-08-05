using Apps.Document360.Handlers;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Requests;

public class DownloadArticleRequest : WorkspaceOptionalIdentifier, IDownloadContentInput
{
    [Display("Article ID", Description = "The article to download. The workspace decides which articles are listed.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string ContentId { get; set; } = string.Empty;

    [Display("Locale", Description = "The language of the translation to download, for example en, fr or fr-ca.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
}

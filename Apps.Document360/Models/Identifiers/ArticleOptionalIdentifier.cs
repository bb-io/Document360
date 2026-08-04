using Apps.Document360.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Identifiers;

public class ArticleOptionalIdentifier : WorkspaceOptionalIdentifier
{
    [Display("Article ID", Description = "The article to work with. The workspace decides which articles are listed.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string ArticleId { get; set; } = string.Empty;

    [Display("Locale",
        Description = "The language of the translation to work with, for example en, fr or fr-ca.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
}

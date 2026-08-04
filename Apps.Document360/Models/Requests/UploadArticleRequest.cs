using Apps.Document360.Handlers;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Requests;

public class UploadArticleRequest : WorkspaceIdentifier, IUploadContentInput
{
    [Display("File",
        Description = "The translated file to write back. Accepts the HTML file from 'Download article', or an XLIFF file.")]
    public FileReference Content { get; set; } = default!;

    [Display("Locale", Description = "The language of the translation to write, for example fr or fr-ca.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;

    [Display("Article ID",
        Description = "The article to write to. Left empty, it is taken from the file.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string? ContentId { get; set; }

    [Display("User ID",
        Description =
            "The team account credited when a published or read-only version has to be copied into a new draft. " +
            "Left empty, the account that created the article is used.")]
    [DataSource(typeof(TeamAccountDataHandler))]
    public string? UserId { get; set; }
}

using Apps.Document360.Handlers;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Document360.Models.Requests;

public class CreateArticleRequest : WorkspaceIdentifier
{
    [Display("Title", Description = "The article title. It also seeds the article address and the SEO title.")]
    public string Title { get; set; } = string.Empty;

    [Display("Category ID", Description = "The category the article is filed under.")]
    [DataSource(typeof(CategoryDataHandler))]
    public string CategoryId { get; set; } = string.Empty;

    [Display("User ID",
        Description = "The team account recorded as the author, and credited with publishing when 'Publish' is on.")]
    [DataSource(typeof(TeamAccountDataHandler))]
    public string UserId { get; set; } = string.Empty;

    [Display("File", Description = "The file holding the article body, as markdown or HTML text.")]
    public FileReference File { get; set; } = default!;

    [Display("Editor format",
        Description =
            "The format the body is stored in. Left empty, it is taken from the file type, or from the project default.")]
    [StaticDataSource(typeof(EditorFormatDataHandler))]
    public string? EditorFormat { get; set; }

    [Display("Slug",
        Description = "A custom address for the article, unique within the workspace. Left empty, it is built from the title.")]
    public string? Slug { get; set; }

    [Display("Order", Description = "The position of the article inside its category. Left empty, it is added last.")]
    public int? Order { get; set; }

    [Display("Publish",
        Description =
            "Put the new article live on the knowledge base site right away. Left empty or off, it stays an " +
            "editable draft that a later 'Publish article' step can put live.")]
    public bool? Publish { get; set; }

    [Display("Publish message",
        Description = "A note recorded in the version history. Only used when 'Publish' is on.")]
    public string? PublishMessage { get; set; }
}

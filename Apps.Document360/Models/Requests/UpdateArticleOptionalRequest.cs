using Apps.Document360.Handlers;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Document360.Models.Requests;

public class UpdateArticleOptionalRequest : ArticleOptionalIdentifier
{
    [Display("Title", Description = "A new title for this translation. Left empty, the title is kept.")]
    public string? Title { get; set; }

    [Display("File", Description = "A file holding the new article body. Supply either a file or content, not both.")]
    public FileReference? File { get; set; }

    [Display("Content", Description = "The new article body as text, for short updates that need no file.")]
    public string? Content { get; set; }

    [Display("Translation status", Description = "How far along this translation is.")]
    [StaticDataSource(typeof(TranslationStatusDataHandler))]
    public string? TranslationStatus { get; set; }

    [Display("Hidden", Description = "Turn on to hide the article from the knowledge base navigation.")]
    public bool? Hidden { get; set; }

    [Display("Category ID",
        Description = "Move the article to another category. Left empty, it stays where it is.")]
    [DataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Order", Description = "The position of the article inside its category.")]
    public int? Order { get; set; }

    [Display("Description",
        Description = "The article description shown in listings, up to 250 characters. This is not the SEO description.")]
    public string? Description { get; set; }

    [Display("Tags", Description = "Replaces the tags of this translation. Left empty, the tags are kept.")]
    public IEnumerable<string>? Tags { get; set; }

    [Display("Custom field IDs",
        Description = "The custom fields to write. Give one value under 'Custom field values' for each field listed here.")]
    [DataSource(typeof(CustomFieldDataHandler))]
    public IEnumerable<string>? CustomFieldIds { get; set; }

    [Display("Custom field values",
        Description = "The values for the custom fields above, in the same order. For a dropdown, use the option ID.")]
    public IEnumerable<string>? CustomFieldValues { get; set; }

    [Display("SEO title", Description = "The title search engines show for this article.")]
    public string? SeoTitle { get; set; }

    [Display("Meta description", Description = "The description search engines show for this article.")]
    public string? MetaDescription { get; set; }

    [Display("User ID",
        Description =
            "The team account credited when a published or read-only version has to be copied into a new draft. " +
            "Left empty, the account that created the article is used.")]
    [DataSource(typeof(TeamAccountDataHandler))]
    public string? UserId { get; set; }
}

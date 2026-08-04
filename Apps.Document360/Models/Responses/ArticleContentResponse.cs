using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Responses;


public class ArticleContentResponse : IDownloadContentOutput
{
    [Display("File")] public FileReference Content { get; set; } = default!;

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Editor format", Description = "Whether the article body is kept as markdown or as HTML.")]
    public string EditorFormat { get; set; } = string.Empty;

    [Display("Version number", Description = "The version the content was read from, or written to.")]
    public int VersionNumber { get; set; }

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Translation status")] public string TranslationStatus { get; set; } = string.Empty;

    [Display("Is fallback content",
        Description =
            "Download only. Turned on when Document360 served the fallback language because this locale has no " +
            "translation of its own yet.")]
    public bool IsFallbackContent { get; set; }

    [Display("Was copied to new draft",
        Description =
            "Upload only. Turned on when the version being written was read-only and had to be copied into a new " +
            "draft first.")]
    public bool WasCopiedToNewDraft { get; set; }

    [Display("Updated fields",
        Description = "Upload only. The fields that actually changed. Empty when the translation was already up to date.")]
    public List<string> UpdatedFields { get; set; } = new();
}

using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class UpdateArticleResponse
{
    public UpdateArticleResponse()
    {
    }

    public UpdateArticleResponse(ArticleDto article, ArticleSettingsDto? settings, string locale, bool wasCopiedToNewDraft)
    {
        ArticleId = article.Id;
        Locale = locale;
        Title = article.Title ?? string.Empty;
        Content = ArticleContent.Select(article);
        Description = article.Description ?? string.Empty;
        ArticleUrl = article.Url ?? string.Empty;
        Status = Document360Enums.ArticleStatus(article.Status);
        TranslationStatus = Document360Enums.TranslationStatus(article.TranslationOption);
        VersionNumber = article.VersionNumber ?? 0;
        LatestVersion = article.LatestVersion ?? 0;
        PublishedVersion = article.PublicVersion ?? 0;
        Tags = settings?.Tags ?? [];
        CustomFields = (settings?.CustomFields ?? []).Select(field => new CustomFieldValueResponse(field)).ToList();
        WasCopiedToNewDraft = wasCopiedToNewDraft;
        ModifiedAt = article.ModifiedAt;
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Content")] public string Content { get; set; } = string.Empty;

    [Display("Description")] public string Description { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Translation status")] public string TranslationStatus { get; set; } = string.Empty;

    [Display("Version number", Description = "The draft version that now holds the content.")]
    public int VersionNumber { get; set; }

    [Display("Latest version")] public int LatestVersion { get; set; }

    [Display("Published version",
        Description = "When this is lower than the latest version, the article has changes that are not live yet.")]
    public int PublishedVersion { get; set; }

    [Display("Tags")] public List<string> Tags { get; set; } = new();

    [Display("Custom fields")] public List<CustomFieldValueResponse> CustomFields { get; set; } = new();

    [Display("Was copied to new draft",
        Description =
            "Turned on when the version being updated was read-only and had to be copied into a new draft first.")]
    public bool WasCopiedToNewDraft { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }
}

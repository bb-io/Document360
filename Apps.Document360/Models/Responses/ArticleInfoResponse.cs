using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class ArticleInfoResponse
{
    public ArticleInfoResponse()
    {
    }

    public ArticleInfoResponse(ArticleDto article, ArticleSettingsDto? settings, string locale)
    {
        ArticleId = article.Id;
        Locale = locale;
        Title = article.Title ?? string.Empty;
        Slug = article.Slug ?? string.Empty;
        ArticleUrl = article.Url ?? string.Empty;
        Content = ArticleContent.Select(article);
        EditorFormat = Document360Enums.EditorFormat(article.ContentType);
        Description = article.Description ?? string.Empty;
        WorkspaceId = article.ProjectVersionId ?? string.Empty;
        CategoryId = article.CategoryId ?? string.Empty;
        CategoryType = article.CategoryType ?? 0;
        Order = article.Order ?? 0;
        Hidden = article.Hidden ?? false;
        Status = Document360Enums.ArticleStatus(article.Status);
        WorkflowStatusId = article.CurrentWorkflowStatusId ?? string.Empty;
        VersionNumber = article.VersionNumber ?? 0;
        LatestVersion = article.LatestVersion ?? 0;
        PublishedVersion = article.PublicVersion ?? 0;
        Visibility = Document360Enums.Visibility(article.SecurityVisibility);
        TranslationStatus = Document360Enums.TranslationStatus(article.TranslationOption);
        IsFallbackContent = article.IsFallBackContent ?? false;
        CreatedBy = article.CreatedBy ?? string.Empty;
        CreatedAt = article.CreatedAt;
        ModifiedAt = article.ModifiedAt;
        AvailableLocales = (article.AvailableLanguages ?? [])
            .Select(language => new AvailableLocaleResponse(language)).ToList();
        Authors = (article.Authors ?? []).Select(author => new ArticleAuthorResponse(author)).ToList();
        CustomFieldLabels = (article.CustomFieldsData ?? new Dictionary<string, string?>())
            .ToDictionary(entry => entry.Key, entry => entry.Value ?? string.Empty);
        Tags = settings?.Tags ?? [];
        CustomFields = (settings?.CustomFields ?? []).Select(field => new CustomFieldValueResponse(field)).ToList();
        SeoTitle = settings?.SeoTitle ?? string.Empty;
        MetaDescription = settings?.Description ?? string.Empty;
        ExcludedFromSearch = settings?.ExcludeFromSearch ?? false;
        ExcludedFromAiSearch = settings?.ExcludeFromAiSearch ?? false;
        ExcludedFromExternalSearch = settings?.ExcludeFromExternalSearch ?? false;
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Slug")] public string Slug { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Content", Description = "The article body as text. Use 'Download article' to get it as a file.")]
    public string Content { get; set; } = string.Empty;

    [Display("Editor format")] public string EditorFormat { get; set; } = string.Empty;

    [Display("Description")] public string Description { get; set; } = string.Empty;

    [Display("Workspace ID")] public string WorkspaceId { get; set; } = string.Empty;

    [Display("Category ID", Description = "Keep this when creating localized versions so the structure is mirrored.")]
    public string CategoryId { get; set; } = string.Empty;

    [Display("Category type")] public int CategoryType { get; set; }

    [Display("Order")] public int Order { get; set; }

    [Display("Hidden")] public bool Hidden { get; set; }

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Workflow status ID")] public string WorkflowStatusId { get; set; } = string.Empty;

    [Display("Version number")] public int VersionNumber { get; set; }

    [Display("Latest version")] public int LatestVersion { get; set; }

    [Display("Published version",
        Description = "When this is lower than the latest version, the article has changes that are not live yet.")]
    public int PublishedVersion { get; set; }

    [Display("Visibility")] public string Visibility { get; set; } = string.Empty;

    [Display("Translation status")] public string TranslationStatus { get; set; } = string.Empty;

    [Display("Is fallback content")] public bool IsFallbackContent { get; set; }

    [Display("Created by", Description = "The team account that created the article.")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }

    [Display("Available locales",
        Description = "The other languages of this article and how far each translation has come.")]
    public List<AvailableLocaleResponse> AvailableLocales { get; set; } = new();

    [Display("Authors")] public List<ArticleAuthorResponse> Authors { get; set; } = new();

    [Display("Tags")] public List<string> Tags { get; set; } = new();

    [Display("Custom fields", Description = "Custom field values as stored, ready to be written back.")]
    public List<CustomFieldValueResponse> CustomFields { get; set; } = new();

    [Display("Custom field labels",
        Description = "Custom field values by field name, with dropdown values shown as their labels.")]
    public Dictionary<string, string> CustomFieldLabels { get; set; } = new();

    [Display("SEO title")] public string SeoTitle { get; set; } = string.Empty;

    [Display("Meta description")] public string MetaDescription { get; set; } = string.Empty;

    [Display("Excluded from search")] public bool ExcludedFromSearch { get; set; }

    [Display("Excluded from AI search")] public bool ExcludedFromAiSearch { get; set; }

    [Display("Excluded from external search")]
    public bool ExcludedFromExternalSearch { get; set; }
}

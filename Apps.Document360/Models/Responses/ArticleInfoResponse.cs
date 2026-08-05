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
        EditorFormat = Document360Enums.EditorFormat(article.ContentType);
        Description = article.Description ?? string.Empty;
        WorkspaceId = article.ProjectVersionId ?? string.Empty;
        Status = Document360Enums.ArticleStatus(article.Status);
        CreatedBy = article.CreatedBy ?? string.Empty;
        CreatedAt = article.CreatedAt;
        ModifiedAt = article.ModifiedAt;
        Tags = settings?.Tags ?? [];
        SeoTitle = settings?.SeoTitle ?? string.Empty;
        MetaDescription = settings?.Description ?? string.Empty;
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Slug")] public string Slug { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Editor format")] public string EditorFormat { get; set; } = string.Empty;

    [Display("Description")] public string Description { get; set; } = string.Empty;

    [Display("Workspace ID")] public string WorkspaceId { get; set; } = string.Empty;
    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Created by", Description = "The team account that created the article.")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }
    [Display("Tags")] public List<string> Tags { get; set; } = new();

    [Display("SEO title")] public string SeoTitle { get; set; } = string.Empty;

    [Display("Meta description")] public string MetaDescription { get; set; } = string.Empty;
}

using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class CreateArticleResponse
{
    public CreateArticleResponse()
    {
    }

    public CreateArticleResponse(ArticleDto article, string locale)
    {
        ArticleId = article.Id;
        Title = article.Title ?? string.Empty;
        Slug = article.Slug ?? string.Empty;
        Locale = locale;
        ArticleUrl = article.Url ?? string.Empty;
        WorkspaceId = article.ProjectVersionId ?? string.Empty;
        CategoryId = article.CategoryId ?? string.Empty;
        Status = Document360Enums.ArticleStatus(article.Status);
        VersionNumber = article.VersionNumber ?? 0;
        LatestVersion = article.LatestVersion ?? 0;
        PublishedVersion = article.PublicVersion ?? 0;
        EditorFormat = Document360Enums.EditorFormat(article.ContentType);
        Hidden = article.Hidden ?? false;
        CreatedAt = article.CreatedAt;
        ModifiedAt = article.ModifiedAt;
    }

    [Display("Article ID", Description = "Use this in every later step that works with the article.")]
    public string ArticleId { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Slug")] public string Slug { get; set; } = string.Empty;

    [Display("Locale", Description = "The language the body was written to, which is the workspace's own language.")]
    public string Locale { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Workspace ID")] public string WorkspaceId { get; set; } = string.Empty;

    [Display("Category ID")] public string CategoryId { get; set; } = string.Empty;

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Version number")] public int VersionNumber { get; set; }

    [Display("Latest version")] public int LatestVersion { get; set; }

    [Display("Published version", Description = "The version that is live on the site, or 0 while nothing is live.")]
    public int PublishedVersion { get; set; }

    [Display("Editor format")] public string EditorFormat { get; set; } = string.Empty;

    [Display("Hidden")] public bool Hidden { get; set; }

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }
}

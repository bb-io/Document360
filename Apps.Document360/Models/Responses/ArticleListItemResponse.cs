using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Responses;


public class ArticleListItemResponse : IDownloadContentInput
{
    public ArticleListItemResponse()
    {
    }

    public ArticleListItemResponse(ArticleListItemDto dto, string workspaceId = "")
    {
        ContentId = dto.Id;
        WorkspaceId = workspaceId;
        Title = dto.Title ?? string.Empty;
        Slug = dto.Slug ?? string.Empty;
        ArticleUrl = dto.Url ?? string.Empty;
        Locale = dto.LanguageCode ?? string.Empty;
        Status = Document360Enums.ArticleStatus(dto.Status);
        EditorFormat = Document360Enums.EditorFormat(dto.ContentType);
        TranslationStatus = Document360Enums.TranslationStatus(dto.TranslationOption);
        Visibility = Document360Enums.Visibility(dto.SecurityVisibility);
        PublishedVersion = dto.PublicVersion ?? 0;
        LatestVersion = dto.LatestVersion ?? 0;
        Order = dto.Order ?? 0;
        Hidden = dto.Hidden ?? false;
        ExcludedFromExternalSearch = dto.ExcludeFromExternalSearch ?? false;
        WorkflowStatusId = dto.CurrentWorkflowStatusId ?? string.Empty;
        CreatedAt = dto.CreatedAt;
        ModifiedAt = dto.ModifiedAt;
    }

    [Display("Article ID")] public string ContentId { get; set; } = string.Empty;

    [Display("Workspace ID", Description = "The workspace the article lives in.")]
    public string WorkspaceId { get; set; } = string.Empty;

    [Display("Title")] public string Title { get; set; } = string.Empty;

    [Display("Slug")] public string Slug { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Status")] public string Status { get; set; } = string.Empty;

    [Display("Editor format")] public string EditorFormat { get; set; } = string.Empty;

    [Display("Translation status")] public string TranslationStatus { get; set; } = string.Empty;

    [Display("Visibility")] public string Visibility { get; set; } = string.Empty;

    [Display("Published version",
        Description = "The version that is live on the site, or 0 when the article has never been published.")]
    public int PublishedVersion { get; set; }

    [Display("Latest version")] public int LatestVersion { get; set; }

    [Display("Order")] public int Order { get; set; }

    [Display("Hidden")] public bool Hidden { get; set; }

    [Display("Excluded from external search")]
    public bool ExcludedFromExternalSearch { get; set; }

    [Display("Workflow status ID")] public string WorkflowStatusId { get; set; } = string.Empty;

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }
}

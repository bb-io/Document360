using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class WorkspaceResponse
{
    public WorkspaceResponse()
    {
    }

    public WorkspaceResponse(ProjectVersionDto dto)
    {
        WorkspaceId = dto.Id;
        Name = dto.VersionCodeName ?? string.Empty;
        Slug = dto.Slug ?? string.Empty;
        VersionNumber = dto.VersionNumber ?? 0;
        BaseVersionNumber = dto.BaseVersionNumber ?? 0;
        IsMainVersion = dto.IsMainVersion ?? false;
        IsBeta = dto.IsBeta ?? false;
        IsPublic = dto.IsPublic ?? false;
        IsDeprecated = dto.IsDeprecated ?? false;
        WorkspaceType = Document360Enums.WorkspaceType(dto.VersionType);
        Order = dto.Order ?? 0;
        CreatedAt = dto.CreatedAt;
        ModifiedAt = dto.ModifiedAt;
        Locales = (dto.LanguageVersions ?? []).Select(language => new WorkspaceLanguageResponse(language)).ToList();
    }

    [Display("Workspace ID")] public string WorkspaceId { get; set; } = string.Empty;

    [Display("Name")] public string Name { get; set; } = string.Empty;

    [Display("Slug")] public string Slug { get; set; } = string.Empty;

    [Display("Version number")] public double VersionNumber { get; set; }

    [Display("Base version number")] public double BaseVersionNumber { get; set; }

    [Display("Is main version")] public bool IsMainVersion { get; set; }

    [Display("Is beta")] public bool IsBeta { get; set; }

    [Display("Is public")] public bool IsPublic { get; set; }

    [Display("Is deprecated")] public bool IsDeprecated { get; set; }

    [Display("Workspace type")] public string WorkspaceType { get; set; } = string.Empty;

    [Display("Order")] public int Order { get; set; }

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Modified at")] public DateTime? ModifiedAt { get; set; }

    [Display("Locales", Description = "The languages enabled for this workspace.")]
    public List<WorkspaceLanguageResponse> Locales { get; set; } = new();
}

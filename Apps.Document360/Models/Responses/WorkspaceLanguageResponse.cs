using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class WorkspaceLanguageResponse
{
    public WorkspaceLanguageResponse()
    {
    }

    public WorkspaceLanguageResponse(ProjectVersionLanguageDto dto)
    {
        LanguageId = dto.Id;
        Locale = dto.Code;
        Name = dto.Name ?? string.Empty;
        IsDefault = dto.SetAsDefault ?? false;
        Hidden = dto.Hidden ?? false;
        IsRightToLeft = dto.EnableRtl ?? false;
        SiteProtectionLevel = dto.SiteProtectionLevel ?? 0;
    }

    [Display("Language ID")] public string LanguageId { get; set; } = string.Empty;

    [Display("Locale", Description = "The code to use wherever an action asks for a locale.")]
    public string Locale { get; set; } = string.Empty;

    [Display("Name")] public string Name { get; set; } = string.Empty;

    [Display("Is default", Description = "Turned on for the source language of the workspace.")]
    public bool IsDefault { get; set; }

    [Display("Hidden")] public bool Hidden { get; set; }

    [Display("Is right to left")] public bool IsRightToLeft { get; set; }

    [Display("Site protection level")] public int SiteProtectionLevel { get; set; }
}

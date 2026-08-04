using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class LocaleResponse
{
    public LocaleResponse()
    {
    }

    public LocaleResponse(WorkspaceLocaleDto dto)
    {
        LanguageId = dto.LanguageId;
        Locale = dto.LanguageCode;
        Name = dto.LanguageName ?? string.Empty;
        IsDefault = dto.IsSetAsDefault ?? false;
        CountryFlagCode = dto.CountryFlagCode ?? string.Empty;
    }

    [Display("Language ID")] public string LanguageId { get; set; } = string.Empty;

    [Display("Locale", Description = "The code to use wherever an action asks for a locale.")]
    public string Locale { get; set; } = string.Empty;

    [Display("Name")] public string Name { get; set; } = string.Empty;

    [Display("Is default", Description = "Turned on for the source language of the workspace.")]
    public bool IsDefault { get; set; }

    [Display("Country flag code")] public string CountryFlagCode { get; set; } = string.Empty;
}

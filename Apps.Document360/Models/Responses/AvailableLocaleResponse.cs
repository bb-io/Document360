using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class AvailableLocaleResponse
{
    public AvailableLocaleResponse()
    {
    }

    public AvailableLocaleResponse(AvailableLanguageDto dto)
    {
        Locale = dto.LangCode;
        ArticleUrl = dto.Url ?? string.Empty;
        TranslationStatus = Document360Enums.TranslationStatus(dto.TranslationStatus);
    }

    [Display("Locale")] public string Locale { get; set; } = string.Empty;

    [Display("Article URL")] public string ArticleUrl { get; set; } = string.Empty;

    [Display("Translation status")] public string TranslationStatus { get; set; } = string.Empty;
}

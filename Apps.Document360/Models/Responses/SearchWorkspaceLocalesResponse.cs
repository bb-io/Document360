using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class SearchWorkspaceLocalesResponse
{
    [Display("Locales")] public List<LocaleResponse> Locales { get; set; } = new();

    [Display("Total count")] public int TotalCount { get; set; }
}

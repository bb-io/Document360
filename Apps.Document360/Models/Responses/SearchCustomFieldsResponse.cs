using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class SearchCustomFieldsResponse
{
    [Display("Custom fields")] public List<CustomFieldDefinitionResponse> CustomFields { get; set; } = new();

    [Display("Total count")] public int TotalCount { get; set; }
}

using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class CustomFieldValueResponse
{
    public CustomFieldValueResponse()
    {
    }

    public CustomFieldValueResponse(CustomFieldValueDto dto)
    {
        FieldId = dto.FieldId;
        Name = dto.Name ?? string.Empty;
        Type = Document360Enums.CustomFieldType(dto.Type);
        Value = dto.Value ?? string.Empty;
        Options = (dto.Options ?? []).Select(option => new CustomFieldOptionResponse(option)).ToList();
    }

    [Display("Field ID")] public string FieldId { get; set; } = string.Empty;

    [Display("Name")] public string Name { get; set; } = string.Empty;

    [Display("Type")] public string Type { get; set; } = string.Empty;

    [Display("Value", Description = "The stored value. For a dropdown this is the option ID, not its label.")]
    public string Value { get; set; } = string.Empty;

    [Display("Options")] public List<CustomFieldOptionResponse> Options { get; set; } = new();
}

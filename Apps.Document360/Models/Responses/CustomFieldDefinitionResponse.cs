using Apps.Document360.Models.Dtos;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class CustomFieldDefinitionResponse
{
    public CustomFieldDefinitionResponse()
    {
    }

    public CustomFieldDefinitionResponse(CustomFieldDefinitionDto dto)
    {
        FieldId = dto.FieldId;
        Name = dto.Name ?? string.Empty;
        Type = Document360Enums.CustomFieldType(dto.Type);
        Placeholder = dto.Placeholder ?? string.Empty;
        HelpText = dto.HelpText ?? string.Empty;
        MaxLength = dto.MaxLength;
        Options = (dto.Options ?? []).Select(option => new CustomFieldOptionResponse(option)).ToList();
    }

    [Display("Field ID", Description = "Use this when telling 'Update article' which custom field to write.")]
    public string FieldId { get; set; } = string.Empty;

    [Display("Name")] public string Name { get; set; } = string.Empty;

    [Display("Type")] public string Type { get; set; } = string.Empty;

    [Display("Placeholder")] public string Placeholder { get; set; } = string.Empty;

    [Display("Help text")] public string HelpText { get; set; } = string.Empty;

    [Display("Max length", Description = "The longest value a text field accepts.")]
    public int? MaxLength { get; set; }

    [Display("Options", Description = "The choices of a dropdown field, empty for other field types.")]
    public List<CustomFieldOptionResponse> Options { get; set; } = new();
}

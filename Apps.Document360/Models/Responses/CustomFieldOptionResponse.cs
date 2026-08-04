using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class CustomFieldOptionResponse
{
    public CustomFieldOptionResponse()
    {
    }

    public CustomFieldOptionResponse(CustomFieldOptionDto dto)
    {
        OptionId = dto.Id;
        Label = dto.Label ?? string.Empty;
    }

    [Display("Option ID", Description = "The value to send when this option should be selected.")]
    public string OptionId { get; set; } = string.Empty;

    [Display("Label")] public string Label { get; set; } = string.Empty;
}

using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// One entry of the <c>errors</c> array. Document360 returns this in both a snake_case and a camelCase
/// envelope; <c>description</c> is spelled the same in both, and it is the only human-usable text.
/// </summary>
public class ErrorDetailDto
{
    [JsonProperty("description")] public string? Description { get; set; }
}

using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class ErrorDetailDto
{
    [JsonProperty("description")] public string? Description { get; set; }
}

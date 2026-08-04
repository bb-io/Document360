using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;


public class Document360Response<T>
{
    [JsonProperty("data")] public T? Data { get; set; }

    [JsonProperty("result")] public T? Result { get; set; }

    [JsonProperty("url")] public string? Url { get; set; }

    [JsonProperty("pagination")] public PaginationDto? Pagination { get; set; }

    [JsonProperty("success")] public bool Success { get; set; }

    [JsonProperty("errors")] public List<ErrorDetailDto>? Errors { get; set; }

    public T? Payload => Data ?? Result;

    public bool HasErrorDetails => Errors is not null && Errors.Any(x => !string.IsNullOrWhiteSpace(x.Description));

    public string? ErrorMessage => Errors is null
        ? null
        : string.Join(" ", Errors.Select(x => x.Description).Where(x => !string.IsNullOrWhiteSpace(x)));
}

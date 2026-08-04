using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class ArticleAuthorResponse
{
    public ArticleAuthorResponse()
    {
    }

    public ArticleAuthorResponse(ArticleAuthorDto dto)
    {
        UserId = dto.Id;
        FirstName = dto.FirstName ?? string.Empty;
        LastName = dto.LastName ?? string.Empty;
        Email = dto.EmailId ?? string.Empty;
    }

    [Display("User ID")] public string UserId { get; set; } = string.Empty;

    [Display("First name")] public string FirstName { get; set; } = string.Empty;

    [Display("Last name")] public string LastName { get; set; } = string.Empty;

    [Display("Email")] public string Email { get; set; } = string.Empty;
}

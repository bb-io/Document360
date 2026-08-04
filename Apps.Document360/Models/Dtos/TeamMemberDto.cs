using Newtonsoft.Json;

namespace Apps.Document360.Models.Dtos;

/// <summary>
/// A team account from <c>GET /v2/Teams</c> — the only endpoint that returns its payload under <c>result</c>
/// instead of <c>data</c>.
/// </summary>
public class TeamMemberDto
{
    [JsonProperty("user_id")] public string UserId { get; set; } = string.Empty;

    [JsonProperty("first_name")] public string? FirstName { get; set; }

    [JsonProperty("last_name")] public string? LastName { get; set; }

    [JsonProperty("email_id")] public string? EmailId { get; set; }

    [JsonProperty("portal_role")] public string? PortalRole { get; set; }

    [JsonProperty("last_login_at")] public DateTime? LastLoginAt { get; set; }

    public string DisplayName
    {
        get
        {
            var name = $"{FirstName} {LastName}".Trim();
            return string.IsNullOrWhiteSpace(name) ? EmailId ?? UserId : name;
        }
    }
}

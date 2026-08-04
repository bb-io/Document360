using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api.Services;

/// <summary>Team accounts of the project.</summary>
public class TeamService(Document360Client client)
{
    private const int PageSize = 100;
    private const int MaxPages = 20;

    /// <summary>
    /// All team accounts. This is the one endpoint that returns its payload under <c>result</c> instead of
    /// <c>data</c>, which the envelope handles, and it pages with skip/take.
    /// </summary>
    public Task<List<TeamMemberDto>> GetTeamMembersAsync(CancellationToken cancellationToken = default) =>
        client.GetSkipTakeListAsync<TeamMemberDto>((skip, take) => new RestRequest("/v2/Teams", Method.Get)
            .AddQueryParameter("skip", skip.ToString())
            .AddQueryParameter("take", take.ToString()), PageSize, MaxPages, cancellationToken);
}

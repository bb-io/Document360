using Apps.Document360.Api;
using Apps.Document360.Extensions;
using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Document360.Handlers;

/// <summary>
/// Team accounts of the project. Accounts that have signed in at least once are listed first, because
/// Document360 refuses to record accounts that never accepted their invitation as an article author.
/// </summary>
public class TeamAccountDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int MaxPages = 20;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        // The one endpoint that returns its payload under 'result' instead of 'data', which the envelope handles,
        // and the one that pages with skip/take rather than page numbers.
        var members = await Client.GetSkipTakeListAsync<TeamMemberDto>(
            (skip, take) => new RestRequest("/v2/Teams", Method.Get)
                .AddQueryParameter("skip", skip.ToString())
                .AddQueryParameter("take", take.ToString()),
            Document360Client.MaxPageSize, MaxPages, cancellationToken);

        return members
            .OrderByDescending(member => member.LastLoginAt.HasValue)
            .ThenByDescending(member => member.LastLoginAt)
            .Select(member => new DataSourceItem(member.UserId,
                member.DisplayName == member.EmailId || string.IsNullOrWhiteSpace(member.EmailId)
                    ? member.DisplayName
                    : $"{member.DisplayName} ({member.EmailId})"))
            .Matching(context);
    }
}

using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>
/// Team accounts of the project. Accounts that have signed in at least once are listed first, because
/// Document360 refuses to record accounts that never accepted their invitation as an article author.
/// </summary>
public class TeamAccountDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var members = await Teams.GetTeamMembersAsync(cancellationToken);

        return members
            .OrderByDescending(member => member.LastLoginAt.HasValue)
            .ThenByDescending(member => member.LastLoginAt)
            .Select(member => new
            {
                member.UserId,
                Label = member.DisplayName == member.EmailId || string.IsNullOrWhiteSpace(member.EmailId)
                    ? member.DisplayName
                    : $"{member.DisplayName} ({member.EmailId})"
            })
            .Where(item => context.SearchString is null ||
                           item.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(item => new DataSourceItem(item.UserId, item.Label));
    }
}

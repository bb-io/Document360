using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

public class WorkspaceDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var workspaces = await Workspaces.GetWorkspacesAsync(cancellationToken);

        return workspaces
            .Select(workspace => new
            {
                workspace.Id,
                Label = workspace.VersionType == 1
                    ? $"{workspace.VersionCodeName} ({Document360Enums.WorkspaceType(workspace.VersionType)})"
                    : workspace.VersionCodeName ?? workspace.Id
            })
            .Where(item => context.SearchString is null ||
                           item.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(item => new DataSourceItem(item.Id, item.Label));
    }
}

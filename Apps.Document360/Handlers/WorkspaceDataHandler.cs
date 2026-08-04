using Apps.Document360.Api;
using Apps.Document360.Extensions;
using Apps.Document360.Providers;
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
        var workspaces = await ProjectProvider.GetWorkspacesAsync(Client, cancellationToken);

        return workspaces
            .Select(workspace => new DataSourceItem(workspace.Id,

                workspace.VersionType == 1
                    ? $"{workspace.VersionCodeName} ({Document360Enums.WorkspaceType(workspace.VersionType)})"
                    : workspace.VersionCodeName ?? workspace.Id))
            .Matching(context);
    }
}

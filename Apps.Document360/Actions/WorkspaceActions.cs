using Apps.Document360.Api;
using Apps.Document360.Models.Identifiers;
using Apps.Document360.Models.Responses;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Actions;

[ActionList("Workspaces")]
public class WorkspaceActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search workspaces",
        Description = "Get the workspaces of the knowledge base project, together with the languages each supports.")]
    public async Task<SearchWorkspacesResponse> SearchWorkspaces()
    {
        var workspaces = await ProjectProvider.GetWorkspacesAsync(Client);
        var results = workspaces.Select(workspace => new WorkspaceResponse(workspace)).ToList();

        return new SearchWorkspacesResponse { Workspaces = results, TotalCount = results.Count };
    }

    [Action("Search workspace locales", Description = "Get the languages enabled for one workspace.")]
    public async Task<SearchWorkspaceLocalesResponse> SearchWorkspaceLocales(
        [ActionParameter] WorkspaceIdentifier identifier)
    {
        Validate.Workspace(identifier);

        var locales = await ProjectProvider.GetWorkspaceLocalesAsync(Client, identifier.WorkspaceId);
        var results = locales.Select(locale => new LocaleResponse(locale)).ToList();

        return new SearchWorkspaceLocalesResponse { Locales = results, TotalCount = results.Count };
    }
}

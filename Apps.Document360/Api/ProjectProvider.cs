using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api;

/// <summary>
/// The project-level reads that more than one kind of caller needs. Each one is shared for the same reason: an
/// action hands the list to the user, and a data source turns that same list into a dropdown.
/// </summary>
public static class ProjectProvider
{
    /// <summary>
    /// The workspaces of the project, which Document360 calls project versions.
    /// Shared by 'Search workspaces' and the workspace and locale data sources.
    /// </summary>
    public static Task<List<ProjectVersionDto>> GetWorkspacesAsync(Document360Client client,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<ProjectVersionDto>(new RestRequest("/v2/ProjectVersions", Method.Get), cancellationToken);

    /// <summary>
    /// The locales enabled on one workspace.
    /// Shared by 'Search workspace locales', the locale checks of the article actions and the locale data source.
    /// </summary>
    public static Task<List<WorkspaceLocaleDto>> GetWorkspaceLocalesAsync(Document360Client client, string workspaceId,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<WorkspaceLocaleDto>(new RestRequest($"/v2/Language/{workspaceId}", Method.Get),
            cancellationToken);

    /// <summary>
    /// The project's custom field definitions.
    /// Shared by 'Search custom fields' and the custom field data source.
    /// </summary>
    public static Task<List<CustomFieldDefinitionDto>> GetCustomFieldDefinitionsAsync(Document360Client client,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<CustomFieldDefinitionDto>(new RestRequest("/v2/customfields", Method.Get),
            cancellationToken);
}

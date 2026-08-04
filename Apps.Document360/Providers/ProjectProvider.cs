using Apps.Document360.Api;
using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Providers;


public static class ProjectProvider
{


    public static Task<List<ProjectVersionDto>> GetWorkspacesAsync(Document360Client client,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<ProjectVersionDto>(new RestRequest("/v2/ProjectVersions", Method.Get), cancellationToken);


    public static Task<List<WorkspaceLocaleDto>> GetWorkspaceLocalesAsync(Document360Client client, string workspaceId,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<WorkspaceLocaleDto>(new RestRequest($"/v2/Language/{workspaceId}", Method.Get),
            cancellationToken);


    public static Task<List<CustomFieldDefinitionDto>> GetCustomFieldDefinitionsAsync(Document360Client client,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<CustomFieldDefinitionDto>(new RestRequest("/v2/customfields", Method.Get),
            cancellationToken);
}

using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api.Services;

/// <summary>Workspaces — project versions in Document360 — together with their locales and categories.</summary>
public class WorkspaceService(Document360Client client)
{
    public Task<List<ProjectVersionDto>> GetWorkspacesAsync(CancellationToken cancellationToken = default) =>
        client.GetListAsync<ProjectVersionDto>(new RestRequest("/v2/ProjectVersions", Method.Get), cancellationToken);

    public Task<List<WorkspaceLocaleDto>> GetWorkspaceLocalesAsync(string workspaceId,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<WorkspaceLocaleDto>(new RestRequest($"/v2/Language/{workspaceId}", Method.Get),
            cancellationToken);

    public Task<List<CategoryDto>> GetCategoriesAsync(string workspaceId, string? langCode = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/ProjectVersions/{workspaceId}/categories", Method.Get)
            .AddQueryParameter("excludeArticles", "true");

        if (!string.IsNullOrWhiteSpace(langCode)) request.AddQueryParameter("langCode", langCode);

        return client.GetListAsync<CategoryDto>(request, cancellationToken);
    }
}

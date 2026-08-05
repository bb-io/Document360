using Blackbird.Applications.Sdk.Common;

namespace Apps.Document360.Models.Responses;

public class SearchWorkspacesResponse
{
    [Display("Workspaces")] public List<WorkspaceResponse> Workspaces { get; set; } = new();

    [Display("Total count")] public int TotalCount { get; set; }
}

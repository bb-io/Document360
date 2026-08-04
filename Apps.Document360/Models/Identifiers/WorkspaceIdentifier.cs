using Apps.Document360.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Identifiers;

public class WorkspaceIdentifier
{
    [Display("Workspace ID", Description = "The workspace, called a project version in Document360, to work in.")]
    [DataSource(typeof(WorkspaceDataHandler))]
    public string WorkspaceId { get; set; } = string.Empty;
}

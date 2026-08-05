using Apps.Document360.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Identifiers;

public class WorkspaceOptionalIdentifier
{
    [Display("Workspace ID",
        Description =
            "The workspace, called a project version in Document360, to work in. Required by the actions that " +
            "work on a whole workspace; on the others it only narrows the lists of articles and locales offered.")]
    [DataSource(typeof(WorkspaceDataHandler))]
    public string? WorkspaceId { get; set; }
}

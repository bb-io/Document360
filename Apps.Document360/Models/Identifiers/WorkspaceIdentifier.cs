using Apps.Document360.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Identifiers;

public class WorkspaceIdentifier
{
    /// <remarks>
    /// Optional, because most article endpoints are addressed by article ID alone — Document360 resolves the
    /// workspace itself and reports it back on the article. Only the actions that address a whole workspace
    /// really need it, and those check it themselves.
    /// </remarks>
    [Display("Workspace ID",
        Description =
            "The workspace, called a project version in Document360, to work in. Required by the actions that " +
            "work on a whole workspace; on the others it only narrows the lists of articles and locales offered.")]
    [DataSource(typeof(WorkspaceDataHandler))]
    public string? WorkspaceId { get; set; }
}

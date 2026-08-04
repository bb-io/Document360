using Apps.Document360.Handlers;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Polling;

public class ArticlesCreatedFilter
{
    [Display("Workspace ID",
        Description =
            "The workspace, called a project version in Document360, to work in. Required by the actions that " +
            "work on a whole workspace; on the others it only narrows the lists of articles and locales offered.")]
    [DataSource(typeof(WorkspaceDataHandler))]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Display("Locale", Description = "The language to watch. Left empty, the workspace's own language is watched.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }

    [Display("Publication status",
        Description = "Trigger only for articles that are live on the site, or only for those that are not.")]
    [StaticDataSource(typeof(PublicationStatusDataHandler))]
    public string? PublicationStatus { get; set; }

    [Display("Tags", Description = "Trigger only for articles carrying at least one of these tags.")]
    public IEnumerable<string>? Tags { get; set; }

    [Display("Visibility", Description = "Trigger only for articles with this protection level.")]
    [StaticDataSource(typeof(VisibilityDataHandler))]
    public string? Visibility { get; set; }
}

public class ArticlesPublishedFilter
{
    [Display("Workspace ID",
        Description =
            "The workspace, called a project version in Document360, to work in. Required by the actions that " +
            "work on a whole workspace; on the others it only narrows the lists of articles and locales offered.")]
    [DataSource(typeof(WorkspaceDataHandler))]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Display("Locale", Description = "The language to watch. Left empty, the workspace's own language is watched.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }

    [Display("Tags", Description = "Trigger only for articles carrying at least one of these tags.")]
    public IEnumerable<string>? Tags { get; set; }

    [Display("Visibility", Description = "Trigger only for articles with this protection level.")]
    [StaticDataSource(typeof(VisibilityDataHandler))]
    public string? Visibility { get; set; }
}

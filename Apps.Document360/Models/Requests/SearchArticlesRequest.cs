using Apps.Document360.Handlers;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Requests;

public class SearchArticlesRequest : WorkspaceIdentifier
{
    [Display("Locale",
        Description = "The language whose translation state is reported. Left empty, the workspace's own language is used.")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }

    [Display("Publication status", Description = "Keep only articles that are live on the site, or only those that are not.")]
    [StaticDataSource(typeof(PublicationStatusDataHandler))]
    public string? PublicationStatus { get; set; }

    [Display("Visibility", Description = "Keep only articles with this protection level.")]
    [StaticDataSource(typeof(VisibilityDataHandler))]
    public string? Visibility { get; set; }

    [Display("Translation status",
        Description = "Keep only articles in this translation state for the locale above. A locale is required.")]
    [StaticDataSource(typeof(TranslationStatusDataHandler))]
    public string? TranslationStatus { get; set; }

    [Display("Hidden", Description = "Keep only hidden articles, or only visible ones.")]
    public bool? Hidden { get; set; }

    [Display("Modified after", Description = "Keep only articles changed after this moment.")]
    public DateTime? ModifiedAfter { get; set; }

    [Display("Max results", Description = "The largest number of articles to return. Left empty, all matches are returned.")]
    public int? MaxResults { get; set; }
}

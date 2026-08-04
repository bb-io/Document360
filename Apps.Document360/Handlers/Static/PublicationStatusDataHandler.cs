using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Handlers.Static;

/// <summary>
/// Whether an article is live on the knowledge base site. This is decided by the published version number
/// rather than by the article status, because an article can be live and still have a newer draft.
/// </summary>
public class PublicationStatusDataHandler : IStaticDataSourceItemHandler
{
    public const string Draft = "draft";
    public const string Published = "published";

    public IEnumerable<DataSourceItem> GetData() =>
    [
        new(Draft, "Draft - never published"),
        new(Published, "Published - live on the site")
    ];
}

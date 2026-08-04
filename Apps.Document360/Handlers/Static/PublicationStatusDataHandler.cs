using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Handlers.Static;


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

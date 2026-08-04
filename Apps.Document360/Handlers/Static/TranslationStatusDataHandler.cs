using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Handlers.Static;

public class TranslationStatusDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("0", "None"),
        new("1", "Needs translation"),
        new("2", "Translated"),
        new("3", "In progress")
    ];
}

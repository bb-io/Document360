using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Handlers.Static;

public class EditorFormatDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("0", "Markdown"),
        new("2", "Advanced WYSIWYG")
    ];
}

using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Extensions;


public static class DataSourceExtensions
{
    private const int MaxItems = 50;

    public static IEnumerable<DataSourceItem> Matching(this IEnumerable<DataSourceItem> items,
        DataSourceContext context) =>
        items
            .Where(item => string.IsNullOrWhiteSpace(context.SearchString) ||
                           item.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(MaxItems);
}

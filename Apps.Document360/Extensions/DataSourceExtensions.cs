using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Extensions;

/// <summary>
/// Every data source of the app narrows its options the same way, because Document360 has no search endpoint for
/// any of them: fetch, keep what matches what the user typed, and never hand back more than a dropdown can show.
/// </summary>
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

using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>Versions of an article translation, newest first. Subclasses say which article and locale to look at.</summary>
public abstract class ArticleVersionDataHandlerBase(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var (articleId, locale) = Target;
        if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(locale)) return [];

        var versions = await Articles.GetArticleVersionsAsync(articleId, locale, cancellationToken);

        return versions
            .OrderByDescending(version => version.VersionNumber)
            .Select(version => new
            {
                Value = version.VersionNumber.ToString(),
                Label = BuildLabel(version.VersionNumber, Document360Enums.ArticleStatus(version.Status),
                    version.ModifiedAt)
            })
            .Where(item => context.SearchString is null ||
                           item.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(item => new DataSourceItem(item.Value, item.Label));
    }

    protected abstract (string ArticleId, string Locale) Target { get; }

    private static string BuildLabel(int versionNumber, string status, DateTime? modifiedAt)
    {
        var label = $"Version {versionNumber}";
        if (!string.IsNullOrEmpty(status)) label += $" - {status}";
        if (modifiedAt.HasValue) label += $" - {modifiedAt.Value:yyyy-MM-dd}";

        return label;
    }
}

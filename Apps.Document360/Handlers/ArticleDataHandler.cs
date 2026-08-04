using Apps.Document360.Api;
using Apps.Document360.Extensions;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>
/// Articles of the selected workspace. Document360 offers no title search, so the list is fetched and filtered
/// here; a workspace can hold hundreds of articles, which is why an Article ID can also be typed or mapped
/// from an earlier workflow step instead of picked.
/// </summary>
public class ArticleDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceIdentifier workspace)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int MaxPagesToScan = 5;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspace.WorkspaceId)) return [];

        var articles = await ArticleProvider.GetArticlesAsync(Client, workspace.WorkspaceId,
            maxPages: MaxPagesToScan, cancellationToken: cancellationToken);

        return articles
            .Select(article => new DataSourceItem(article.Id,
                string.IsNullOrWhiteSpace(article.Title) ? article.Id : article.Title))
            .Matching(context);
    }
}

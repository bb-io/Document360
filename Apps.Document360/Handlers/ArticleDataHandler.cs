using Apps.Document360.Extensions;
using Apps.Document360.Models.Identifiers;
using Apps.Document360.Providers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

public class ArticleDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceOptionalIdentifier workspaceOptional)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int MaxPagesToScan = 5;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceOptional.WorkspaceId))
        {
            throw new PluginMisconfigurationException("Please provide workspace ID first.");
        }

        var articles = await ArticleProvider.GetArticlesAsync(Client, workspaceOptional.WorkspaceId,
            maxPages: MaxPagesToScan, cancellationToken: cancellationToken);

        return articles
            .Select(article => new DataSourceItem(article.Id,
                string.IsNullOrWhiteSpace(article.Title) ? article.Id : article.Title))
            .Matching(context);
    }
}

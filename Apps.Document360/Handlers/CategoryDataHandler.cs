using Apps.Document360.Extensions;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Document360.Handlers;


public class CategoryDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceOptionalIdentifier workspaceOptional)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceOptional.WorkspaceId))
        {
            throw new PluginMisconfigurationException("Please provide workspace ID first.");
        }

        var request = new RestRequest($"/v2/ProjectVersions/{workspaceOptional.WorkspaceId}/categories", Method.Get)
            .AddQueryParameter("excludeArticles", "true");

        var roots = await Client.GetListAsync<CategoryDto>(request, cancellationToken);
        var items = new List<DataSourceItem>();
        Flatten(roots, string.Empty, items);

        return items.Matching(context);
    }

    private static void Flatten(IEnumerable<CategoryDto> categories, string parentPath, List<DataSourceItem> target)
    {
        foreach (var category in categories)
        {
            var name = string.IsNullOrWhiteSpace(category.Name) ? category.Id : category.Name;
            var path = string.IsNullOrEmpty(parentPath) ? name : $"{parentPath} / {name}";

            target.Add(new DataSourceItem(category.Id, path));

            if (category.ChildCategories is { Count: > 0 }) Flatten(category.ChildCategories, path, target);
        }
    }
}

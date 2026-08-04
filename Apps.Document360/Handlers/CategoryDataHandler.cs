using Apps.Document360.Extensions;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Document360.Handlers;

/// <summary>
/// Categories of the selected workspace. Document360 returns a tree and category names repeat across branches,
/// so the tree is flattened into full paths such as "Hardware / Accessories / Vehicle cameras".
/// </summary>
public class CategoryDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceIdentifier workspace)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspace.WorkspaceId)) return [];

        // The articles hanging off the tree are not needed to name a category, and a workspace can hold hundreds.
        var request = new RestRequest($"/v2/ProjectVersions/{workspace.WorkspaceId}/categories", Method.Get)
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

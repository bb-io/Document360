using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

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

        var roots = await Workspaces.GetCategoriesAsync(workspace.WorkspaceId, cancellationToken: cancellationToken);
        var flattened = new List<(string Id, string Path)>();
        Flatten(roots, string.Empty, flattened);

        return flattened
            .Where(item => context.SearchString is null ||
                           item.Path.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(item => new DataSourceItem(item.Id, item.Path));
    }

    private static void Flatten(IEnumerable<CategoryDto> categories, string parentPath,
        List<(string Id, string Path)> target)
    {
        foreach (var category in categories)
        {
            var name = string.IsNullOrWhiteSpace(category.Name) ? category.Id : category.Name;
            var path = string.IsNullOrEmpty(parentPath) ? name : $"{parentPath} / {name}";

            target.Add((category.Id, path));

            if (category.ChildCategories is { Count: > 0 }) Flatten(category.ChildCategories, path, target);
        }
    }
}

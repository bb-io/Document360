using Apps.Document360.Api;
using Apps.Document360.Extensions;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>
/// Locales enabled on the selected workspace. When no workspace has been chosen yet, every locale of the
/// project is offered so the dropdown is never empty.
/// </summary>
public class LocaleDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceIdentifier workspace)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var locales = string.IsNullOrWhiteSpace(workspace.WorkspaceId)
            ? await GetProjectLocalesAsync(cancellationToken)
            : (await ProjectProvider.GetWorkspaceLocalesAsync(Client, workspace.WorkspaceId, cancellationToken))
            .Select(locale => (Code: locale.LanguageCode, Name: locale.LanguageName ?? locale.LanguageCode))
            .ToList();

        return locales
            .Where(locale => !string.IsNullOrWhiteSpace(locale.Code))
            .DistinctBy(locale => locale.Code, StringComparer.OrdinalIgnoreCase)
            .Select(locale => new DataSourceItem(locale.Code, $"{locale.Name} - {locale.Code}"))
            .Matching(context);
    }

    private async Task<List<(string Code, string Name)>> GetProjectLocalesAsync(CancellationToken cancellationToken)
    {
        var workspaces = await ProjectProvider.GetWorkspacesAsync(Client, cancellationToken);

        return workspaces
            .SelectMany(item => item.LanguageVersions ?? [])
            .Select(language => (Code: language.Code, Name: language.Name ?? language.Code))
            .ToList();
    }
}

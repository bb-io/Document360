using Apps.Document360.Extensions;
using Apps.Document360.Models.Identifiers;
using Apps.Document360.Providers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

public class LocaleDataHandler(InvocationContext invocationContext, [ActionParameter] WorkspaceOptionalIdentifier workspaceOptional)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var locales = string.IsNullOrWhiteSpace(workspaceOptional.WorkspaceId)
            ? await GetProjectLocalesAsync(cancellationToken)
            : (await ProjectProvider.GetWorkspaceLocalesAsync(Client, workspaceOptional.WorkspaceId, cancellationToken))
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

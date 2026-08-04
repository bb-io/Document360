using Apps.Document360.Api;
using Apps.Document360.Extensions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>The project's custom field definitions, used to pick which fields an update should write.</summary>
public class CustomFieldDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var definitions = await ProjectProvider.GetCustomFieldDefinitionsAsync(Client, cancellationToken);

        return definitions
            .Select(definition => new DataSourceItem(definition.FieldId,
                string.IsNullOrWhiteSpace(definition.Name) ? definition.FieldId : definition.Name))
            .Matching(context);
    }
}

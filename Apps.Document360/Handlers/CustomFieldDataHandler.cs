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
        var definitions = await CustomFields.GetCustomFieldDefinitionsAsync(cancellationToken);

        return definitions
            .Select(definition => new
            {
                definition.FieldId,
                Label = string.IsNullOrWhiteSpace(definition.Name) ? definition.FieldId : definition.Name
            })
            .Where(item => context.SearchString is null ||
                           item.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(item => new DataSourceItem(item.FieldId, item.Label));
    }
}

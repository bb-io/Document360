using Apps.Document360.Extensions;
using Apps.Document360.Providers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;


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

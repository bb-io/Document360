using Apps.Document360.Models.Responses;
using Apps.Document360.Providers;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Actions;

[ActionList("Custom fields")]
public class CustomFieldActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search custom fields",
        Description =
            "Get the custom fields of the project, with the types and option IDs needed to fill them in on an article.")]
    public async Task<SearchCustomFieldsResponse> SearchCustomFields()
    {
        var definitions = await ProjectProvider.GetCustomFieldDefinitionsAsync(Client);
        var results = definitions.Select(definition => new CustomFieldDefinitionResponse(definition)).ToList();

        return new SearchCustomFieldsResponse { CustomFields = results, TotalCount = results.Count };
    }
}

using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api.Services;

/// <summary>The project's custom field definitions.</summary>
public class CustomFieldService(Document360Client client)
{
    public Task<List<CustomFieldDefinitionDto>> GetCustomFieldDefinitionsAsync(
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<CustomFieldDefinitionDto>(new RestRequest("/v2/customfields", Method.Get),
            cancellationToken);
}

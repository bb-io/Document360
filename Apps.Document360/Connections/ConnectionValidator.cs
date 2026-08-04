using System.Net;
using Apps.Document360.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Document360.Connections;

public class ConnectionValidator(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new Document360Client(authenticationCredentialsProviders.ToArray());
            var response = await client.ExecuteAsync(new RestRequest("/v2/ProjectVersions", Method.Get),
                cancellationToken);


            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message =
                        "Document360 rejected the API token. Check that the token is correct and has not been revoked, " +
                        "and that the API hub URL matches the region your knowledge base is hosted in."
                };


            if (response.ResponseStatus is ResponseStatus.Error)
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message =
                        $"Could not reach Document360 at this address. Check the API hub URL. " +
                        $"({response.ErrorMessage ?? response.ResponseStatus.ToString()})"
                };


            return new ConnectionValidationResponse { IsValid = true, Message = "Success" };
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[Document360] Connection validation failed: {ex.Message}", []);
            return new ConnectionValidationResponse { IsValid = false, Message = ex.Message };
        }
    }
}

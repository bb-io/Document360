using Apps.Document360.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Document360.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "API token",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredsNames.BaseUrl)
                {
                    DisplayName = "API hub URL",
                    Description =
                        "The regional API hub of your knowledge base. Pick the one that matches your region, or type your own if it is not listed.",
                    DataItems =
                    [
                        new("https://apihub.document360.io", "Default - apihub.document360.io"),
                        new("https://apihub.us.document360.io", "United States - apihub.us.document360.io")
                    ]
                },
                new(CredsNames.ApiToken)
                {
                    DisplayName = "API token",
                    Description =
                        "Created in the Document360 portal under Settings > Knowledge base portal > API tokens. The token is tied to a single project.",
                    Sensitive = true
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values) => values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)
        ).ToList();
}

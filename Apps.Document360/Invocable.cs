using Apps.Document360.Api;
using Apps.Document360.Api.Services;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360;

public class Invocable : BaseInvocable
{
    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new Document360Client(Creds);
        Articles = new ArticleService(Client);
        Workspaces = new WorkspaceService(Client);
        CustomFields = new CustomFieldService(Client);
        Teams = new TeamService(Client);
    }

    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected Document360Client Client { get; }

    protected ArticleService Articles { get; }

    protected WorkspaceService Workspaces { get; }

    protected CustomFieldService CustomFields { get; }

    protected TeamService Teams { get; }
}

using Apps.Document360.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360;


public class Invocable : BaseInvocable
{
    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new Document360Client(Creds);
    }

    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected Document360Client Client { get; }
}

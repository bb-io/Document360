using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;


public class ArticleVersionDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] ArticleIdentifier article)
    : ArticleVersionDataHandlerBase(invocationContext)
{
    protected override (string ArticleId, string Locale) Target => (article.ArticleId, article.Locale);
}

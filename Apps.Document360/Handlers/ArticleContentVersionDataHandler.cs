using Apps.Document360.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

public class ArticleContentVersionDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] DownloadArticleRequest request)
    : ArticleVersionDataHandlerBase(invocationContext)
{
    protected override (string ArticleId, string Locale) Target => (request.ContentId, request.Locale);
}

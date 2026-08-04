using Apps.Document360.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Document360.Handlers;

/// <summary>Versions of the article "Download article" points at, where the article id is the content id.</summary>
public class ArticleContentVersionDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] DownloadArticleRequest request)
    : ArticleVersionDataHandlerBase(invocationContext)
{
    protected override (string ArticleId, string Locale) Target => (request.ContentId, request.Locale);
}

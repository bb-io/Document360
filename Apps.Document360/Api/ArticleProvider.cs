using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api;

/// <summary>
/// The three article reads that more than one kind of caller needs. Every other article endpoint is built where it
/// is used — these would otherwise be written out two or three times over.
/// </summary>
public static class ArticleProvider
{
    private const int MaxPages = 50;

    /// <summary>
    /// The articles of a workspace, across every category.
    /// Shared by 'Search articles', both article polling events and the article data source.
    /// </summary>
    public static Task<List<ArticleListItemDto>> GetArticlesAsync(Document360Client client, string workspaceId,
        string? langCode = null, int? securityVisibility = null, int maxPages = MaxPages,
        CancellationToken cancellationToken = default) =>
        client.GetPagedListAsync<ArticleListItemDto>(page =>
        {
            var request = new RestRequest($"/v2/ProjectVersions/{workspaceId}/articles", Method.Get)
                .AddQueryParameter("page", page.ToString())
                .AddQueryParameter("hitsPerPage", Document360Client.MaxPageSize.ToString());

            if (!string.IsNullOrWhiteSpace(langCode)) request.AddQueryParameter("langCode", langCode);
            if (securityVisibility.HasValue)
                request.AddQueryParameter("securityVisibility", securityVisibility.Value.ToString());

            return request;
        }, maxPages, cancellationToken);

    /// <summary>
    /// Tags, SEO fields and custom field values, which the article endpoint itself does not return.
    /// Shared by the article actions and the tag filter of the polling events.
    /// </summary>
    public static Task<ArticleSettingsDto> GetArticleSettingsAsync(Document360Client client, string articleId,
        string langCode, CancellationToken cancellationToken = default) =>
        client.GetDataAsync<ArticleSettingsDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/settings", Method.Get), cancellationToken);

    /// <summary>
    /// The versions of an article translation.
    /// Shared by 'Publish article', which needs the newest one, and the version data sources.
    /// </summary>
    public static Task<List<ArticleVersionDto>> GetArticleVersionsAsync(Document360Client client, string articleId,
        string langCode, CancellationToken cancellationToken = default) =>
        client.GetListAsync<ArticleVersionDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/versions", Method.Get), cancellationToken);
}
